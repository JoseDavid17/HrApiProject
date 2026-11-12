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

export const hrModules = {
  '/empleados': {
    title: 'Empleados',
    singular: 'empleado',
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
      column('hireDate', 'Fecha de contratación', { format: (value) => value.slice(0, 10) }),
      column('salary', 'Salario', { numeric: true, format: (value) => salaryFormat.format(value) }),
      column('commissionPct', 'Comisión', {
        numeric: true,
        format: (value) => commissionFormat.format(value),
        filterPlaceholder: 'Comisión en decimal (ej. 0.25)',
      }),
      column('id', 'ID', { numeric: true }),
    ],
    createFields: [
      field('employeeId', 'ID de empleado', { type: 'number', required: true }),
      field('firstName', 'Nombre', { required: true, maxLength: 20 }),
      field('lastName', 'Apellido', { required: true, maxLength: 25 }),
      field('email', 'Correo', { required: true, maxLength: 25 }),
      field('phoneNumber', 'Teléfono', { maxLength: 20 }),
      field('hireDate', 'Fecha de contratación', { type: 'date', required: true }),
      field('jobId', 'Puesto', {
        required: true,
        catalog: { endpoint: 'Jobs', paginated: true, labelField: 'jobTitle', valueField: 'jobId' },
      }),
      field('departmentId', 'Departamento', {
        catalog: { endpoint: 'Departments', paginated: true, labelField: 'departmentName', valueField: 'departmentId' },
      }),
      field('salary', 'Salario', { type: 'number', step: '0.01' }),
      field('commissionPct', 'Comisión (decimal)', { type: 'number', step: '0.01', max: 1 }),
    ],
  },
  '/departamentos': {
    title: 'Departamentos',
    description: 'Consulta los departamentos y los códigos de sus responsables y locaciones.',
    endpoint: 'Departments',
    dataKey: 'departmentId',
    paginated: true,
    columns: [
      column('departmentName', 'Departamento'),
      column('managerId', 'ID de responsable', { numeric: true }),
      column('locationId', 'ID de locación', { numeric: true }),
      column('departmentId', 'ID', { numeric: true }),
    ],
  },
  '/regiones': {
    title: 'Regiones',
    singular: 'región',
    description: 'Consulta las regiones que agrupan los países registrados.',
    endpoint: 'Region',
    dataKey: 'id',
    columns: [column('name', 'Región'), column('id', 'ID', { numeric: true })],
    createFields: [
      field('regionId', 'ID de región', { type: 'number', required: true }),
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
