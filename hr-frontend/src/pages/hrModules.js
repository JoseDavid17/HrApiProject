import { createElement } from 'react';
import OrganizationChartPage from './OrganizationChartPage';

const column = (field, header, options = {}) => ({ field, header, ...options });
const field = (name, label, options = {}) => ({ name, label, ...options });
const salaryFormat = new Intl.NumberFormat('es-HN', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});
const commissionFormat = new Intl.NumberFormat('es-HN', {
  style: 'percent',
  maximumFractionDigits: 2,
});

const employeeContactFields = [
  field('firstName', 'Nombre', { maxLength: 20 }),
  field('lastName', 'Apellido', { required: true, maxLength: 25 }),
  field('email', 'Correo', { required: true, maxLength: 25 }),
  field('phoneNumber', 'Teléfono', { maxLength: 20 }),
];
const employeePayFields = [
  field('salary', 'Salario', { type: 'number', step: '0.01' }),
  field('commissionPct', 'Comisión (decimal)', { type: 'number', step: '0.01', max: 1 }),
];

const departmentLocationField = field('locationId', 'Locación', {
  catalog: { endpoint: 'Locations', paginated: true, labelField: 'city', valueField: 'locationID' },
});

const departmentManagerField = field('managerId', 'Responsable', {
  catalog: { endpoint: 'Employee', paginated: true, labelField: 'fullName', valueField: 'id' },
});

function supervisorForm(employee, rows) {
  const supervisors = new Map();

  for (const { supervisorId, fullSupervisorName, supervisorDepartmentId, supervisorDepartmentName } of rows) {
    if (!Number.isInteger(supervisorId) || !Number.isInteger(supervisorDepartmentId) ||
        supervisorId === employee.employeeId || supervisorId === employee.supervisorId) continue;

    const departmentName = supervisorDepartmentName || `Departamento ${supervisorDepartmentId}`;
    supervisors.set(supervisorId, {
      id: supervisorId,
      name: fullSupervisorName,
      departmentId: supervisorDepartmentId,
      departmentName,
      label: `${fullSupervisorName} · ${departmentName} · #${supervisorId}`,
    });
  }

  return {
    title: `Cambiar supervisor de ${employee.fullEmployeeName}`,
    submitLabel: 'Confirmar cambio',
    fields: [field('supervisorId', 'Nuevo supervisor', {
      required: true,
      catalog: {
        options: [...supervisors.values()].sort((first, second) => first.label.localeCompare(second.label, 'es-HN')),
        labelField: 'label',
        valueField: 'id',
        emptyMessage: 'Sin supervisores disponibles',
      },
    })],
    confirmation: ({ supervisorId }) => {
      const supervisor = supervisors.get(supervisorId);
      if (!supervisor) return null;

      const departmentMessage = employee.departmentId === supervisor.departmentId
        ? 'El empleado permanecerá en '
        : 'El empleado se moverá a ';
      const supervisorName = createElement('strong', null, supervisor.name);
      const departmentName = createElement('strong', null, supervisor.departmentName);

      return createElement('div', null,
        createElement('div', null, '¿Confirmas asignar a ', supervisorName, ' como supervisor?'),
        createElement('div', null, departmentMessage, departmentName, '.'),
      );
    },
    prepareRequest: ({ supervisorId }) => ({
      body: supervisorId,
      params: { departmentId: supervisors.get(supervisorId).departmentId },
    }),
  };
}

export const hrModules = {
  '/empleados': {
    title: 'Empleados',
    singular: 'empleado',
    recordLabel: 'fullName',
    description: 'Consulta los empleados del esquema HR de Oracle.',
    endpoint: 'Employee',
    dataKey: 'id',
    paginated: true,
    columns: [
      column('fullName', 'Nombre completo', { avatar: true }),
      column('jobTitle', 'Puesto'),
      column('departmentName', 'Departamento'),
      column('email', 'Correo'),
      column('phone', 'Teléfono'),
      column('hireDate', 'Fecha de contratación', {
        format: (value) => value.replace(/^(\d{4})-(\d{2})-(\d{2})(?:T.*)?$/, '$3/$2/$1'),
      }),
      column('salary', 'Salario', { numeric: true, format: (value) => salaryFormat.format(value) }),
      column('commissionPct', 'Comisión', {
        numeric: true,
        format: (value) => commissionFormat.format(value),
        filterPlaceholder: 'Comisión en decimal (ej. 0.25)',
      }),
      column('id', 'ID', { numeric: true }),
    ],
    canDelete: true,
    editFields: [
      ...employeeContactFields,
      ...employeePayFields.map((payField) => ({ ...payField, step: 'any' })),
    ],
    createFields: [
      ...employeeContactFields.map((contactField) => contactField.name === 'firstName'
        ? { ...contactField, required: true }
        : contactField),
      field('jobId', 'Puesto', {
        required: true,
        catalog: { endpoint: 'Jobs', paginated: true, labelField: 'jobTitle', valueField: 'jobId' },
      }),
      field('departmentId', 'Departamento', {
        catalog: { endpoint: 'Departments', paginated: true, labelField: 'departmentName', valueField: 'departmentId' },
      }),
      ...employeePayFields,
    ],
  },
  '/empleados/organigrama': {
    title: 'Organigrama de la empresa',
    endpoint: 'Employee/OrganizationChart',
    view: OrganizationChartPage,
  },
  '/empleados/supervisores': {
    title: 'Supervisores',
    description: 'Consulta el supervisor directo de cada empleado.',
    endpoint: 'Employee/Supervisores',
    dataKey: 'employeeId',
    paginated: true,
    readOnly: true,
    rowActions: [{
      key: 'change-supervisor',
      label: 'Cambiar supervisor',
      icon: 'pi pi-user-edit',
      endpoint: 'Employee/ChangeSupervisor',
      disabled: ({ employeeId }) => !Number.isInteger(employeeId),
      disabledMessage: 'El cambio de supervisor requiere que la API devuelva los identificadores del listado.',
      successSummary: 'Supervisor actualizado',
      successDetail: 'El cambio se guardó correctamente.',
      form: supervisorForm,
    }],
    columns: [
      column('fullEmployeeName', 'Empleado', { avatar: true }),
      column('fullSupervisorName', 'Supervisor'),
      column('departmentName', "Departamento")
    ],
  },
  '/departamentos': {
    title: 'Departamentos',
    singular: 'departamento',
    recordLabel: 'departmentName',
    updateMethod: 'PUT',
    rowActions: [{
      key: 'change-manager',
      label: 'Cambiar responsable',
      icon: 'pi pi-user-edit',
      endpoint: 'Departments/ChangeManager',
      successSummary: 'Responsable actualizado',
      successDetail: 'El cambio de responsable se guardó correctamente.',
      form: ({ departmentName }) => ({
        title: `Cambiar responsable de ${departmentName}`,
        fields: [{
          ...departmentManagerField,
          label: 'Nuevo responsable',
          required: true,
          catalog: {
            ...departmentManagerField.catalog,
            filterOptions: (employees) => employees.filter((employee) => employee.departmentName === departmentName),
            emptyMessage: 'Sin empleados en este departamento',
          },
        }],
        prepareRequest: ({ managerId }) => ({ body: managerId }),
      }),
    }],
    description: 'Consulta los departamentos y los códigos de sus responsables y locaciones.',
    endpoint: 'Departments',
    dataKey: 'departmentId',
    paginated: true,
    columns: [
      column('departmentName', 'Departamento'),
      column('fullName', 'Nombre del encargado'),
      column('city', 'Ciudad'),
      column('managerId', 'Código del manager', { numeric: true }),
    ],
    canDelete: true,
    createFields: [
      field('departmentName', 'Nombre del departamento', { required: true, maxLength: 30 }),
      departmentManagerField,
      departmentLocationField,
    ],
    editFields: [
      field('departamentName', 'Nombre del departamento', { required: true, maxLength: 30 }),
      { ...departmentLocationField, allowClear: false },
    ],
    editValues: ({ departmentName, locationId }) => ({ departamentName: departmentName, locationId }),
  },
  '/regiones': {
    title: 'Regiones',
    singular: 'región',
    description: 'Consulta las regiones que agrupan los países registrados.',
    endpoint: 'Region',
    dataKey: 'id',
    columns: [column('name', 'Región'), column('id', 'ID', { numeric: true })],
    createFields: [
      field('regionName', 'Región', { required: true, maxLength: 25 }),
    ],
  },
  '/paises': {
    title: 'Países',
    singular: 'país',
    description: 'Consulta los países registrados y la región a la que pertenecen.',
    endpoint: 'Country',
    dataKey: 'countryId',
    paginated: true,
    columns: [
      column('countryName', 'País'),
      column('region.name', 'Región'),
      column('regionId', 'ID de región', { numeric: true }),
      column('countryId', 'Código'),
    ],
    createFields: [
      field('countryId', 'Código de país', { required: true, minLength: 2, maxLength: 2 }),
      field('countryName', 'País', { required: true, maxLength: 40 }),
      field('regionId', 'ID de región', { type: 'number' }),
    ],
  },
  '/locaciones': {
    title: 'Locaciones',
    description: 'Consulta las direcciones y ciudades de las locaciones registradas.',
    endpoint: 'Locations',
    dataKey: 'locationID',
    paginated: true,
    columns: [
      column('city', 'Ciudad'),
      column('streetAddress', 'Dirección'),
      column('stateProvince', 'Estado o provincia'),
      column('postalCode', 'Código postal'),
      column('locationID', 'ID', { numeric: true }),
    ],
  },
};
