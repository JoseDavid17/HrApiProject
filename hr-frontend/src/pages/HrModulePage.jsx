import { useEffect, useId, useState } from 'react';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputText } from 'primereact/inputtext';
import CrudTable from '../components/CrudTable';
import { apiErrorMessage, createRecord, listRecords } from './hrApi';

function CatalogField({ field, value, onChange, disabled }) {
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
          setResult({ error: apiErrorMessage(error, 'No se pudo cargar la lista.') });
        }
      });
    return () => controller.abort();
  }, [catalog, attempt]);

  const options = result?.options ?? [];
  const placeholder = !result ? 'Cargando opciones…'
    : result.error ? 'Opciones no disponibles'
      : options.length ? 'Seleccionar' : 'Sin opciones registradas';

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
        showClear={!required}
        disabled={disabled || !options.length}
      />
      {result?.error && (
        <div className="crud-form__catalog-error">
          <span role="alert">{result.error}</span>
          <button type="button" disabled={disabled} onClick={() => {
            setResult(null);
            setAttempt((current) => current + 1);
          }}>Reintentar</button>
        </div>
      )}
    </div>
  );
}

function RecordForm({ title, fields, submitLabel, submit, onClose }) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [selections, setSelections] = useState({});

  async function handleSubmit(event) {
    event.preventDefault();
    if (busy) return;
    const formData = new FormData(event.currentTarget);
    const values = Object.fromEntries(fields.map(({ name, type, catalog }) => {
      if (catalog) return [name, selections[name] ?? null];
      const text = String(formData.get(name) ?? '').trim();
      return [name, type === 'number' && text ? Number(text) : text || null];
    }));

    if (fields.some(({ name, required }) => required && values[name] == null)) {
      setError('Completa los campos obligatorios.');
      return;
    }

    setBusy(true);
    setError('');
    try {
      await submit(values);
      onClose();
    } catch (requestError) {
      setError(apiErrorMessage(requestError, 'No se pudo completar la operación.'));
    } finally {
      setBusy(false);
    }
  }

  return (
    <Dialog header={title} visible onHide={onClose} className="crud-dialog"
      closable={!busy} closeOnEscape={!busy} draggable={false} resizable={false}>
      <form className="crud-form" onSubmit={handleSubmit}>
        {fields.map((field) => {
          const { name, label, catalog, type = 'text', ...attributes } = field;
          if (catalog) {
            return <CatalogField key={name} field={field} value={selections[name] ?? null}
              onChange={(value) => setSelections((current) => ({ ...current, [name]: value }))}
              disabled={busy} />;
          }

          return (
            <label className="crud-form__field" key={name}>
              <span>{label}{attributes.required && ' *'}</span>
              <InputText name={name} type={type} {...attributes}
                min={type === 'number' ? 0 : undefined} disabled={busy} />
            </label>
          );
        })}
        {error && <p className="crud-form__error" role="alert">{error}</p>}
        <div className="crud-form__actions">
          <Button type="button" label="Cancelar" outlined disabled={busy} onClick={onClose} />
          <Button type="submit" label={submitLabel} loading={busy} />
        </div>
      </form>
    </Dialog>
  );
}

export default function HrModulePage({ module }) {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [request, setRequest] = useState(0);
  const [dialog, setDialog] = useState(null);
  const [notice, setNotice] = useState('');

  useEffect(() => {
    const controller = new AbortController();
    listRecords(module, controller.signal)
      .then((records) => {
        if (!controller.signal.aborted) setRows(records);
      })
      .catch((requestError) => {
        if (controller.signal.aborted) return;
        if ([401, 403].includes(requestError.response?.status)) setRows([]);
        setError(requestError);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
    return () => controller.abort();
  }, [module, request]);

  function refresh() {
    setError(null);
    setLoading(true);
    setRequest((value) => value + 1);
  }

  async function addRecord(values) {
    await createRecord(module.endpoint, values);
    setNotice('Registro agregado.');
    refresh();
  }

  return (
    <section className="crud-page" aria-labelledby="crud-page-title">
      <h1 id="crud-page-title">{module.title}</h1>
      <p className="crud-page__description">{module.description}</p>
      {error && (
        <div className="crud-notice">
          <span role="alert">{apiErrorMessage(error, 'No se pudieron cargar los registros. Verifica que la API esté disponible.')}</span>
        </div>
      )}
      {notice && <p className="crud-notice" role="status">{notice}</p>}
      <CrudTable
        rows={rows}
        columns={module.columns}
        dataKey={module.dataKey}
        loading={loading}
        onRefresh={refresh}
        onAdd={module.createFields ? () => setDialog('add') : undefined}
        addUnavailableReason={!module.createFields ? 'Este módulo todavía no admite altas en la API.' : undefined}
      />
      {dialog === 'add' && (
        <RecordForm title={`Agregar ${module.singular}`} fields={module.createFields}
          submitLabel="Guardar" submit={addRecord} onClose={() => setDialog(null)} />
      )}
    </section>
  );
}
