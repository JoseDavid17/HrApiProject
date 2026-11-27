using HrApiProject.Data;
using HrApiProject.Domain;
using HrApiProject.DTOs;
using HrApiProject.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Services
{
    public class EmployeeServices : IEmployeeService
    {
        private readonly HrDbContext _context;

        public EmployeeServices(HrDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeDetailsDto>> GetAllEmployees(int pagenumber, int pagesize)
        {

            int registroASaltar = (pagenumber - 1) * pagesize;

            return await _context.Employees
              .OrderBy(e => e.EmployeeId) // 1. Ordenamos (Obligatorio para paginar)
              .Skip(registroASaltar)  // 2. Nos saltamos los de las páginas anteriores
              .Take(pagesize) // 3. Tomamos solo los de esta página
             .Select(e => new EmployeeDetailsDto
             {
                 Id = e.EmployeeId,
                 FullName = e.FirstName + " " + e.LastName,
                 Email = e.Email,
                 Phone = e.Phone,
                 HireDate = e.Hiredate,
                 Salary = e.Salary,
                 CommissionPct = e.CommissionPct,

                 DepartmentName = e.Department!.DepartmentName,

                 JobTitle = e.Jobs != null ? e.Jobs.JobTitle : "Desconocido"
             }).ToListAsync();

        }

        public async Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(int id)
        {

            return await _context.Employees
             .Where(e => e.EmployeeId == id)
             .Select(e => new EmployeeDetailsDto
             {

                 Id = e.EmployeeId,
                 FullName = e.FirstName + " " + e.LastName,
                 Email = e.Email,
                 Phone = e.Phone,
                 HireDate = e.Hiredate,
                 Salary = e.Salary,
                 CommissionPct = e.CommissionPct,
                 DepartmentName = e.Department!.DepartmentName,
                 JobTitle = e.Jobs!.JobTitle
             }).FirstOrDefaultAsync();
        }

        public async Task<EmployeeDetailsDto> CreateEmployeesAsync(EmployeeCreateDto employeeCreateDto)
        {
            var newEmployee = new Employees
            {
                FirstName = employeeCreateDto.FirstName,
                LastName = employeeCreateDto.LastName,
                Email = employeeCreateDto.Email,
                Hiredate = employeeCreateDto.HireDate,
                Phone = employeeCreateDto.PhoneNumber,
                CommissionPct = employeeCreateDto.CommissionPct,
                Salary = employeeCreateDto.Salary,
                DepartmentId = employeeCreateDto.DepartmentId,
                JobId = employeeCreateDto.JobId
            };


            var yaexisteCorreo = await _context.Employees.CountAsync(e => e.Email == employeeCreateDto.Email);

            if (yaexisteCorreo > 0)
            {
                throw new BusinessRuleException($"El correo del empleado {employeeCreateDto.Email} ya le pertenece a un empleado");
            }

            _context.Employees.Add(newEmployee);
            await _context.SaveChangesAsync();

            return (await GetEmployeeByIdAsync(newEmployee.EmployeeId))!;
        }

        public async Task<bool> UpdateDatosEmployee(int id, UpdateDataEmpDTO updatedatosEmpleado)
        {
            var empleadoEntity = await _context.Employees.FindAsync(id);

            if (empleadoEntity == null)
            {
                return false;
            }

            empleadoEntity.FirstName = updatedatosEmpleado.FirstName;
            empleadoEntity.LastName = updatedatosEmpleado.LastName;
            empleadoEntity.Email = updatedatosEmpleado.Email;
            empleadoEntity.Phone = updatedatosEmpleado.PhoneNumber;
            empleadoEntity.Salary = updatedatosEmpleado.Salary;
            empleadoEntity.CommissionPct = updatedatosEmpleado.CommissionPct;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteEmpleado(int id)
        {
            var empleado = await _context.Employees.FindAsync(id);

            if (empleado == null)
            {
                return false;
            }

            var existeEmployee = await _context.JobHistory.CountAsync(j => j.EmployeeId == id);

            if(existeEmployee > 0)
            {
                throw new BusinessRuleException("No se puede eliminar el empleado porque tiene historial asociado");
            }

            _context.Employees.Remove(empleado);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<EmployeeSupervisorsDto>> Supervisores(int pagenumber, int pagesize)
        {
            int registroASaltar = (pagenumber - 1) * pagesize;

            return await _context.Employees
            .AsNoTracking()
            .Where(e => e.ManagerId != null)
            .OrderBy(e => e.EmployeeId)
            .Skip(registroASaltar)
            .Take(pagesize)
            .Select(e => new EmployeeSupervisorsDto
            {
                EmployeeId = e.EmployeeId,
                DepartmentId = e.DepartmentId,
                FullEmployeeName = e.FirstName + " " + e.LastName,
                DepartmentName = e.Department!.DepartmentName,

                SupervisorId = e.ManagerId,

                FullSupervisorName = e.Manager!.FirstName + " " + e.Manager.LastName,
                SupervisorDepartmentId = e.Manager!.DepartmentId,
                SupervisorDepartmentName = e.Manager!.Department!.DepartmentName


            }).ToListAsync();
        }

        public async Task<bool> ChangeSupervisor(int employeeid, int supervisor, int departmentid)
        {
            var employee = await _context.Employees.FindAsync(employeeid);
            if (employee is null)
            {
                return false;
            }

            if (employeeid == supervisor)
            {
                throw new BusinessRuleException(
                    "Un empleado no puede ser su propio supervisor.");
            }

            var newSupervisor = await _context.Employees.FindAsync(supervisor);
            if (newSupervisor is null)
            {
                return false;
            }

            if (newSupervisor.DepartmentId is not int supervisorDepartmentId)
            {
                throw new BusinessRuleException(
                    $"El supervisor {supervisor} no tiene un departamento asignado.");
            }

            if (supervisorDepartmentId != departmentid)
            {
                throw new BusinessRuleException(
                    $"El supervisor {supervisor} pertenece al departamento {supervisorDepartmentId}.");
            }

            employee.ManagerId = newSupervisor.EmployeeId;
            employee.DepartmentId = supervisorDepartmentId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EmployeeOrganizationNodeDto>> Organigrama(int? employeeid = null)
        {

             var employees = await _context.Employees
            .GroupJoin(
                _context.Jobs,
                employee => employee.JobId,
                job => job.JobId,
                (employee, jobs) => new { Employee = employee, Jobs = jobs })
            .SelectMany(
                result => result.Jobs.DefaultIfEmpty(),
                (result, job) => new
                {
                    result.Employee.EmployeeId,
                    result.Employee.ManagerId,
                    result.Employee.FirstName,
                    result.Employee.LastName,
                    JobTitle = job == null ? result.Employee.JobId : job.JobTitle
                })
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ThenBy(employee => employee.EmployeeId)
            .ToListAsync();

            var nodes = employees.Select(employee => new EmployeeOrganizationNodeDto
            {
                EmployeeId = employee.EmployeeId,
                ManagerId = employee.ManagerId,
                FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
                JobTitle = employee.JobTitle
            }).ToList();

            var reportsByManager = nodes.ToLookup(employee => employee.ManagerId);
            var visited = new HashSet<int>();

            void AddReports(EmployeeOrganizationNodeDto manager)
            {
                foreach (var employee in reportsByManager[manager.EmployeeId])
                {
                    if (visited.Add(employee.EmployeeId))
                    {
                        manager.Children.Add(employee);
                        AddReports(employee);
                    }
                }
            }

            var roots = employeeid is int id
                ? nodes.Where(employee => employee.EmployeeId == id)
                : reportsByManager[null];

            var organizationChart = new List<EmployeeOrganizationNodeDto>();

            foreach (var root in roots)
            {
                if (visited.Add(root.EmployeeId))
                {
                    AddReports(root);
                    organizationChart.Add(root);
                }
            }

            return organizationChart;


        }

        public async Task<bool> ChangeDepartment(int employeeid, int departmentid, int supervisorid, string jobid, decimal? salary = null)
        {
            var employee = await _context.Employees.FindAsync(employeeid);

            if(employee is null)
            {
                return false;
            }

            if(employeeid == 100)
            {
                throw new BusinessRuleException("No puedes cambiar de departamento al CEO de la empresa");
            }

            if(employee.DepartmentId == departmentid)
            {
                throw new BusinessRuleException("El empleado ya pertenece al departamento seleccionado");
            }

            var department = await _context.Departments.FindAsync(departmentid);

            if(department is null)
            {
                throw new BusinessRuleException("El departamento seleccionado no existe");
            }

            if (employeeid == supervisorid)
            {
                throw new BusinessRuleException("El empleado no puede ser su propio supervisor");
            }

            var supervisor = await _context.Employees.FindAsync(supervisorid);

            if(supervisor is null)
            {
                throw new BusinessRuleException("El supervisor no existe");
            }

            if (supervisor.DepartmentId != departmentid)
            {
                throw new BusinessRuleException("El supervisor debe pertenecer al departamento destino.");
            }

            // evitar colocar al empleado bajo uno de sus subordinados
            var visited = new HashSet<int>();
            int? currentSupervisorId = supervisorid;

            while (currentSupervisorId.HasValue)
            {
                int currentId = currentSupervisorId.Value;

                if (currentId == employeeid)
                {
                    throw new BusinessRuleException("El supervisor seleccionado depende del empleado. " + "El traslado crearía un ciclo en la jerarquía.");
                }

                if (!visited.Add(currentId))
                {
                    throw new BusinessRuleException(
                        "La jerarquía del supervisor contiene un ciclo " + "que debe corregirse antes del traslado.");
                }

                var currentSupervisor = await _context.Employees
                    .FindAsync(currentId);

                if (currentSupervisor is null)
                {
                    throw new BusinessRuleException("La jerarquía contiene un supervisor inexistente.");
                }

                currentSupervisorId = currentSupervisor.ManagerId;
            }

            if (string.IsNullOrWhiteSpace(jobid))
            {
                throw new BusinessRuleException("Debes seleccionar el nuevo puesto.");
            }

            var job = await _context.Jobs.FindAsync(jobid.Trim());

            if (job is null)
            {
                throw new BusinessRuleException("El puesto seleccionado no existe.");
            }

            if (job.MinSalary is null || job.MaxSalary is null)
            {
                throw new BusinessRuleException("El puesto seleccionado debe tener un salario mínimo " + "y máximo configurados.");
            }

            decimal minimum = job.MinSalary.Value;
            decimal maximum = job.MaxSalary.Value;

            if (minimum > maximum)
            {
                throw new BusinessRuleException("El rango salarial del puesto está mal configurado.");
            }

            decimal? currentSalary = employee.Salary.HasValue ? Convert.ToDecimal(employee.Salary.Value) : null;

            decimal? resultingSalary = salary ?? currentSalary;

            if (resultingSalary is null)
            {
                throw new BusinessRuleException("Debes indicar un salario para realizar el traslado.");
            }

            if (resultingSalary.Value <= 0)
            {
                throw new BusinessRuleException("El salario debe ser mayor que cero.");
            }

            if (resultingSalary.Value < minimum || resultingSalary.Value > maximum)
            {
                throw new BusinessRuleException($"El salario debe estar entre {minimum:N2} y " + $"{maximum:N2} para el puesto {job.JobTitle}. " + "Indica un salario dentro de ese rango.");
            }

            employee.DepartmentId = department.DepartmentId;
            employee.ManagerId = supervisor.EmployeeId;
            employee.JobId = job.JobId;

            if (salary.HasValue)
            {
                employee.Salary = Convert.ToDouble(salary.Value);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<dynamic>> SameHireDate(int pageNumber, int pageSize)
        {
            int paginacion = (pageNumber-1)*pageSize;

            var results = await _context.Employees
                .AsNoTracking()
                .Join(
                    _context.Employees,
                    employee => employee.Hiredate,
                    colleague => colleague.Hiredate,
                    (employee, colleague) => new { employee, colleague }
                )
                .Where(pair => pair.employee.EmployeeId > pair.colleague.EmployeeId)
                .OrderBy(pair => pair.employee.EmployeeId)
                .ThenBy(pair => pair.colleague.EmployeeId)
                .Select(pair => new
                {
                    Employee1 = pair.employee.FirstName + " " + pair.employee.LastName,
                    Employee1HireDate = pair.employee.Hiredate,
                    Employee2 = pair.colleague.FirstName + " " + pair.colleague.LastName,
                    Employee2HireDate = pair.colleague.Hiredate
                })
                .Skip(paginacion)
                .Take(pageSize)
                .ToListAsync();

            return results;
        }
    }
}
