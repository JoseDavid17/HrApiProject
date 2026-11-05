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
                 DepartmentName =  e.Department!.DepartmentName,
                 JobTitle = e.Jobs!.JobTitle 
             }).FirstOrDefaultAsync();
        }

        public async Task<EmployeeDetailsDto> CreateEmployeesAsync(EmployeeCreateDto employeeCreateDto)
        {
            var newEmployee = new Employees
            {
                EmployeeId = employeeCreateDto.EmployeeId,
                FirstName = employeeCreateDto.FirstName,
                LastName = employeeCreateDto.LastName,
                Email = employeeCreateDto.Email,
                Hiredate = employeeCreateDto.HireDate,
                Phone =employeeCreateDto.PhoneNumber,
                CommissionPct= employeeCreateDto.CommissionPct,
                Salary = employeeCreateDto.Salary,
                DepartmentId = employeeCreateDto.DepartmentId,
                JobId = employeeCreateDto.JobId
            };

            var yaexisteempleado = await _context.Employees.CountAsync(e => e.EmployeeId == employeeCreateDto.EmployeeId);

            if (yaexisteempleado > 0)
            {
                throw new BusinessRuleException($"El empleado ' {employeeCreateDto.EmployeeId} ya está registrado.");
            }

            var yaexisteCorreo = await _context.Employees.CountAsync(e=> e.Email == employeeCreateDto.Email);

            if( yaexisteCorreo > 0)
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

            if(empleadoEntity == null)
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

            if(empleado == null)
            {
                return false;
            }

            _context.Employees.Remove(empleado);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
