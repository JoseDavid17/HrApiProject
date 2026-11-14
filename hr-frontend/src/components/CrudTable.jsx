import { cloneElement, Fragment, useState } from 'react';
import { FilterMatchMode, FilterOperator } from 'primereact/api';
import { Avatar } from 'primereact/avatar';
import { Button } from 'primereact/button';
import { Column } from 'primereact/column';
import { DataTable } from 'primereact/datatable';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import './CrudTable.css';

const pageSize = 10;

const paginatorTemplate = {
  layout: 'FirstPageLink PrevPageLink CurrentPageReport PageLinks NextPageLink LastPageLink',
  CurrentPageReport: ({ className, currentPage, totalPages }) => (
    <span className={className} aria-live="polite">
      {totalPages ? currentPage : 0} de {totalPages}
    </span>
  ),
  PageLinks: ({ element, page, currentPage, totalPages, view }) => (
    <Fragment key={page}>
      {page === view.startPage && page > 0 && <span aria-hidden="true">…</span>}
      {cloneElement(element, {
        'aria-label': `Página ${page + 1}`,
        'aria-current': page === currentPage ? 'page' : undefined,
      })}
      {page === view.endPage && page < totalPages - 1 && <span aria-hidden="true">…</span>}
    </Fragment>
  ),
};

function initialFilters(columns) {
  return {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    ...Object.fromEntries(columns.map(({ field, numeric }) => [
      field,
      {
        operator: FilterOperator.AND,
        constraints: [{ value: null, matchMode: numeric ? FilterMatchMode.EQUALS : FilterMatchMode.CONTAINS }],
      },
    ])),
  };
}

function cellValue(record, { field, format }) {
  const value = field.split('.').reduce((current, key) => current?.[key], record);
  if (value == null || value === '') return '—';
  return format ? format(value) : value;
}

function TableCell({ record, column }) {
  const value = cellValue(record, column);
  if (!column.avatar || value === '—') return value;

  const initials = String(value).trim().split(/\s+/).slice(0, 2)
    .map((word) => word.charAt(0)).join('').toUpperCase();

  return (
    <span className="crud-table__identity">
      <Avatar label={initials} shape="circle" aria-hidden="true" />
      <span>{value}</span>
    </span>
  );
}

export default function CrudTable({
  rows, columns, dataKey, loading, onRefresh, onAdd, addUnavailableReason,
  onEdit, additionalActions = [],
}) {
  const [filters, setFilters] = useState(() => initialFilters(columns));
  const [showFilters, setShowFilters] = useState(false);
  const [details, setDetails] = useState(null);
  const [first, setFirst] = useState(0);
  const hasColumnFilters = columns.some(({ field }) =>
    filters[field].constraints.some(({ value }) => value != null && value !== ''),
  );
  const rowActions = [
    { key: 'view', label: 'Ver detalles', icon: 'pi pi-eye', onClick: setDetails },
    { key: 'edit', label: 'Editar', icon: 'pi pi-pencil', onClick: onEdit },
    ...additionalActions,
  ];

  function updateFilters(nextFilters) {
    setFilters(nextFilters);
    setFirst(0);
  }

  const toolbarStart = (
    <>
      <div className="crud-table__search-group">
        <span className="crud-table__search">
          <i className="pi pi-search" aria-hidden="true" />
          <InputText
            type="search"
            placeholder="Buscar..."
            aria-label="Buscar registros"
            value={filters.global.value ?? ''}
            onChange={(event) => updateFilters({
              ...filters,
              global: { ...filters.global, value: event.target.value },
            })}
          />
        </span>
        <Button type="button" icon="pi pi-filter" outlined title="Filtros"
          aria-label="Filtros" aria-pressed={showFilters} data-filtered={hasColumnFilters}
          onClick={() => setShowFilters(!showFilters)} />
        {hasColumnFilters && (
          <Button type="button" icon="pi pi-filter-slash" outlined title="Limpiar filtros"
            aria-label="Limpiar filtros" onClick={() => updateFilters({
              ...initialFilters(columns), global: filters.global,
            })} />
        )}
      </div>
      <Button type="button" icon="pi pi-refresh" outlined title="Actualizar"
        className="crud-table__refresh" aria-label="Actualizar" disabled={loading} onClick={() => {
          setFirst(0);
          onRefresh();
        }} />
    </>
  );

  return (
    <div className="crud-table">
      <DataTable
        value={rows}
        dataKey={dataKey}
        filters={filters}
        onFilter={(event) => updateFilters(event.filters)}
        globalFilterFields={columns.map(({ field }) => field)}
        filterDisplay="menu"
        filterLocale="es-HN"
        loading={loading}
        emptyMessage={loading ? 'Cargando registros…' : 'No se encontraron registros.'}
        paginator
        paginatorPosition="both"
        paginatorLeft={({ className }) => className === 'p-paginator-top' ? toolbarStart : null}
        paginatorRight={({ className }) => className === 'p-paginator-top' && (
          <span className="crud-table__add" title={addUnavailableReason ?? 'Agregar registro'}>
            <Button type="button" icon="pi pi-plus" outlined aria-label="Agregar registro"
              disabled={!onAdd} onClick={onAdd} />
          </span>
        )}
        first={Math.min(first, Math.max(0, Math.ceil(rows.length / pageSize) - 1) * pageSize)}
        rows={pageSize}
        onPage={(event) => setFirst(event.first)}
        paginatorTemplate={paginatorTemplate}
        responsiveLayout="scroll"
        tableStyle={{ minWidth: columns.length > 4 ? '42rem' : '24rem' }}
        rowHover
      >
        {columns.map((column) => (
          <Column
            key={column.field}
            field={column.field}
            header={column.header}
            body={(record) => <TableCell record={record} column={column} />}
            sortable
            filter={showFilters}
            dataType={column.numeric ? 'numeric' : 'text'}
            filterType={column.numeric ? 'number' : 'text'}
            filterMenuClassName="crud-filter-menu"
            filterPlaceholder={column.filterPlaceholder ?? `Filtrar por ${column.header.toLowerCase()}`}
            showFilterOperator={false}
            showAddButton={false}
            showFilterMatchModes={false}
          />
        ))}
        <Column header="Acciones" className="crud-table__actions" body={(record) => (
          <div className="crud-table__action-group">
            {rowActions.map(({ key, label, icon, onClick }) => (
              <span key={key} title={onClick ? label : `${label} no disponible`}>
                <Button
                  type="button"
                  icon={icon}
                  text
                  aria-label={`${label} (registro ${record[dataKey]})`}
                  disabled={!onClick}
                  onClick={onClick ? () => onClick(record) : undefined}
                />
              </span>
            ))}
          </div>
        )} />
      </DataTable>
      <Dialog header="Detalles del registro" visible={Boolean(details)} onHide={() => setDetails(null)}
        className="crud-dialog" draggable={false} resizable={false}>
        <dl className="crud-details">
          {details && columns.map((column) => (
            <div key={column.field}>
              <dt>{column.header}</dt>
              <dd>{cellValue(details, column)}</dd>
            </div>
          ))}
        </dl>
      </Dialog>
    </div>
  );
}
