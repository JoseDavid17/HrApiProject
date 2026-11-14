import { useEffect, useId, useRef, useState } from 'react';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputText } from 'primereact/inputtext';
import CrudTable from '../components/CrudTable';
import { apiErrorMessage, changeDepartmentManager, createRecord, deleteRecord, listRecords, updateRecord } from './hrApi';

function employeeEditValues(employee) {
  const fullName = employee.fullName ?? '';
  const separator = fullName.indexOf(' ');
  return {
    firstName: employee.firstName ?? (separator > 0 ? fullName.slice(0, separator) : ''),
    lastName: employee.lastName ?? (separator >= 0 ? fullName.slice(separator + 1).trim() : fullName),
    email: employee.email,
    phoneNumber: employee.phone,
    salary: employee.salary,
    commissionPct: employee.commissionPct,
  };
}

function CatalogField({ field, value, onChange, disabled, notify, departmentName }) {
  const { label, required, catalog } = field;
  const inputId = useId();
  const [result, setResult] = useState(null);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    listRecords(catalog, controller.signal)
      .then((options) => {
        if (!controller.signal.aborted) setResult({ options });
      })
      .catch((error) => {
        if (!controller.signal.aborted) {
          setResult({ failed: true });
          notify('error', `No se pudo cargar el catálogo: ${label}`,
            apiErrorMessage(error, 'Selecciona Reintentar para volver a cargarlo.'));
        }
      });
    return () => controller.abort();
  }, [catalog, attempt, label, notify]);

  const records = result?.options ?? [];
  const options = departmentName === undefined ? records
    : records.filter((employee) => employee.departmentName === departmentName);
  const emptyMessage = departmentName === undefined
    ? 'Sin opciones registradas' : 'Sin empleados en este departamento';
  const placeholder = !result ? 'Cargando opciones…'
    : result.failed ? 'Opciones no disponibles'
      : options.length ? 'Seleccionar' : emptyMessage;

  return (
    <div className="crud-form__field crud-form__catalog">
      <label id={`${inputId}-label`} htmlFor={inputId}>{label}{required && ' *'}</label>
      <Dropdown
        inputId={inputId}
        ariaLabelledBy={`${inputId}-label`}
        value={value}
        onChange={(event) => onChange(event.value ?? null)}
        options={options}
        optionLabel={catalog.labelField}
        optionValue={catalog.valueField}
        filter
        filterBy={`${catalog.labelField},${catalog.valueField}`}
        filterLocale="es-HN"
        filterPlaceholder={`Buscar ${label.toLowerCase()}`}
        emptyFilterMessage="No se encontraron coincidencias."
        placeholder={placeholder}
        panelClassName="crud-catalog-panel"
        showClear={field.allowClear ?? !required}
        disabled={disabled || !options.length}
      />
      {result?.failed && (
        <button className="crud-form__catalog-retry" type="button" disabled={disabled} onClick={() => {
          setResult(null);
          setAttempt((current) => current + 1);
        }}>Reintentar</button>
      )}
    </div>
  );
}

function RecordForm({ title, fields, submitLabel, submit, onClose, notify, initialValues, nameToReview, departmentName }) {
  const [busy, setBusy] = useState(false);
  const submitInProgress = useRef(false);
  const [selections, setSelections] = useState(() => initialValues ?? {});

  async function handleSubmit(event) {
    event.preventDefault();
    if (submitInProgress.current) return;
    const invalidInput = event.currentTarget.querySelector('input:invalid');
    if (invalidInput) {
      invalidInput.focus();
      notify('warn', 'Revisa el formulario', invalidInput.validationMessage);
      return;
    }
    const formData = new FormData(event.currentTarget);
    const values = Object.fromEntries(fields.map(({ name, type, catalog }) => {
      if (catalog) return [name, selections[name] ?? null];
      const text = String(formData.get(name) ?? '').trim();
      return [name, type === 'number' && text ? Number(text) : text || null];
    }));

    if (fields.some(({ name, required }) => required && values[name] == null)) {
      notify('warn', 'Campos obligatorios', 'Completa los campos marcados con un asterisco.');
      return;
    }

    submitInProgress.current = true;
    setBusy(true);
    try {
      await submit(values);
      onClose();
    } catch (requestError) {
      notify('error', 'No se pudo guardar', apiErrorMessage(requestError, 'Inténtalo de nuevo.'));
    } finally {
      submitInProgress.current = false;
      setBusy(false);
    }
  }

  return (
    <Dialog header={title} visible onHide={onClose} className="crud-dialog"
      closable={!busy} closeOnEscape={!busy} draggable={false} resizable={false}>
      <form className="crud-form" noValidate onSubmit={handleSubmit}>
        {nameToReview && (
          <label className="crud-form__hint">
            <input type="checkbox" required />
            <span>Confirmo que el nombre y apellido de <strong>{nameToReview}</strong> están correctos.</span>
          </label>
        )}
        {fields.map((field) => {
          const { name, label, catalog, type = 'text', ...attributes } = field;
          if (catalog) {
            return <CatalogField key={name} field={field} value={selections[name] ?? null}
              onChange={(value) => setSelections((current) => ({ ...current, [name]: value }))}
              disabled={busy} notify={notify} departmentName={departmentName} />;
          }

          return (
            <label className="crud-form__field" key={name}>
              <span>{label}{attributes.required && ' *'}</span>
              <InputText name={name} type={type} defaultValue={initialValues?.[name] ?? ''} {...attributes}
                min={type === 'number' ? 0 : undefined} disabled={busy} />
            </label>
          );
        })}
        <div className="crud-form__actions">
          <Button type="button" label="Cancelar" outlined disabled={busy} onClick={onClose} />
          <Button type="submit" label={submitLabel} loading={busy} />
        </div>
      </form>
    </Dialog>
  );
}

export default function HrModulePage({ module, notify }) {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [request, setRequest] = useState(0);
  const [dialog, setDialog] = useState(null);
  const [recordToEdit, setRecordToEdit] = useState(null);
  const [recordToDelete, setRecordToDelete] = useState(null);
  const [departmentToManage, setDepartmentToManage] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const deleteInProgress = useRef(false);

  useEffect(() => {
    const controller = new AbortController();
    listRecords(module, controller.signal)
      .then((records) => {
        if (!controller.signal.aborted) setRows(records);
      })
      .catch((requestError) => {
        if (controller.signal.aborted) return;
        if ([401, 403].includes(requestError.response?.status)) setRows([]);
        if (requestError.response?.status !== 401) {
          notify('error', 'No se pudo cargar el listado',
            apiErrorMessage(requestError, 'Verifica que la API esté disponible y vuelve a actualizar.'));
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    return () => controller.abort();
  }, [module, notify, request]);

  function refresh() {
    setLoading(true);
    setRequest((value) => value + 1);
  }

  async function addRecord(values) {
    await createRecord(module.endpoint, values);
    notify('success', 'Registro agregado', 'Los datos se guardaron correctamente.');
    refresh();
  }

  async function saveRecord(values) {
    await updateRecord(module.endpoint, recordToEdit[module.dataKey], values, module.updateMethod);
    notify('success', 'Registro actualizado', 'Los cambios se guardaron correctamente.');
    refresh();
  }

  async function saveManager({ managerId }) {
    await changeDepartmentManager(departmentToManage.departmentId, managerId);
    notify('success', 'Responsable actualizado', 'El cambio de responsable se guardó correctamente.');
    refresh();
  }

  async function confirmDelete() {
    if (!recordToDelete || deleteInProgress.current) return;
    deleteInProgress.current = true;
    setDeleting(true);
    try {
      const id = recordToDelete[module.dataKey];
      await deleteRecord(module.endpoint, id);
      setRows((current) => current.filter((record) => record[module.dataKey] !== id));
      setRecordToDelete(null);
      notify('success', 'Registro eliminado', 'Se quitó el registro del listado.');
      refresh();
    } catch (error) {
      if (error.response?.status === 404) {
        setRecordToDelete(null);
        notify('warn', 'Registro no encontrado', 'Se actualizará el listado.');
        refresh();
      } else {
        notify('error', 'No se pudo eliminar el registro', apiErrorMessage(error, 'Inténtalo de nuevo.'));
      }
    } finally {
      deleteInProgress.current = false;
      setDeleting(false);
    }
  }

  return (
    <section className="crud-page" aria-labelledby="crud-page-title">
      <h1 id="crud-page-title">{module.title}</h1>
      <p className="crud-page__description">{module.description}</p>
      <CrudTable
        rows={rows}
        columns={module.columns}
        dataKey={module.dataKey}
        loading={loading}
        onRefresh={refresh}
        onAdd={module.createFields ? () => setDialog('add') : undefined}
        onEdit={module.editFields ? setRecordToEdit : undefined}
        additionalActions={[
          ...(module.changeManagerFields ? [{
            key: 'change-manager', label: 'Cambiar responsable', icon: 'pi pi-user-edit', onClick: setDepartmentToManage,
          }] : []),
          ...(module.canDelete ? [{
            key: 'delete', label: 'Eliminar', icon: 'pi pi-trash', onClick: setRecordToDelete,
          }] : []),
        ]}
        addUnavailableReason={!module.createFields ? 'Este módulo todavía no admite altas en la API.' : undefined}
      />
      {dialog === 'add' && (
        <RecordForm title={`Agregar ${module.singular}`} fields={module.createFields}
          submitLabel="Guardar" submit={addRecord} onClose={() => setDialog(null)} notify={notify} />
      )}
      {recordToEdit && (
        <RecordForm key={recordToEdit[module.dataKey]} title={`Editar ${module.singular}`} fields={module.editFields}
          initialValues={(module.editValues ?? employeeEditValues)(recordToEdit)}
          nameToReview={!module.editValues && recordToEdit.firstName == null && recordToEdit.lastName == null &&
            recordToEdit.fullName?.trim().split(/\s+/).length > 2 ? recordToEdit.fullName : undefined}
          submitLabel="Guardar cambios" submit={saveRecord} onClose={() => setRecordToEdit(null)}
          notify={notify} />
      )}
      {departmentToManage && (
        <RecordForm title={`Cambiar responsable de ${departmentToManage.departmentName}`}
          fields={module.changeManagerFields} departmentName={departmentToManage.departmentName}
          submitLabel="Guardar cambios" submit={saveManager} onClose={() => setDepartmentToManage(null)}
          notify={notify} />
      )}
      <Dialog header={`Eliminar ${module.singular}`} className="crud-dialog"
        visible={Boolean(recordToDelete)} onHide={() => setRecordToDelete(null)}
        closable={!deleting} closeOnEscape={!deleting} draggable={false} resizable={false}
        footer={(
          <>
            <Button type="button" label="Cancelar" outlined autoFocus disabled={deleting}
              onClick={() => setRecordToDelete(null)} />
            <Button type="button" label="Eliminar" icon="pi pi-trash" loading={deleting} onClick={confirmDelete} />
          </>
        )}>
        <p>¿Quieres eliminar este {module.singular}? <strong>{recordToDelete?.[module.recordLabel] ?? recordToDelete?.[module.dataKey]}</strong></p>
      </Dialog>
    </section>
  );
}
