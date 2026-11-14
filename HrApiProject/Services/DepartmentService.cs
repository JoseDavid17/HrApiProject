using HrApiProject.Data;
using HrApiProject.Domain;
using HrApiProject.DTOs;
using HrApiProject.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HrDbContext _context;

        public DepartmentService(HrDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartments(int pageNumber, int pageSize)
        {
            int resultadosApaginar = (pageNumber - 1) * pageSize;

            return await _context.Departments
             .OrderBy(d => d.DepartmentId)
             .Skip(resultadosApaginar)
             .Take(pageSize)
            .Select(d=> new DepartmentDto {

              DepartmentId = d.DepartmentId,
              DepartmentName=d.DepartmentName,
              FullName =d.Employees.FirstName + ' ' +d.Employees.LastName,
              City = d.Location!.City ,
              ManagerId = d.ManagerId,
            } ).ToListAsync();
        }

        public async Task<IEnumerable<Departments>> GetDepartmentss()
        {



            //return await _context.Departments
            //    .Where(d => d.DepartmentName.StartsWith("I")
            //    )

            //    .Select(d => new DepartmentDto
            //    {
            //        DepartmentId = d.DepartmentId,
            //        DepartmentName = d.DepartmentName,
            //        LocationId = d.LocationId,
            //        Location = new LocationDto
            //        {
            //            LocationID = d.Location!.LocationID,
            //            StreetAddress = d.Location!.StreetAddress,
            //            City = d.Location.City,
            //            StateProvince = d.Location.StateProvince
            //        }
            //    }).ToListAsync();

            return await _context.Departments
               
                .Include(d => d.Location)
                //.Where(d => d.DepartmentName.StartsWith("I"))
                .Where(d => d.Location != null)
                .ToListAsync();


            //return await _context.Departments
            //    .Where(d => d.Location != null)
            //    .Select(d => new Departments
            //    {
            //        DepartmentName = d.DepartmentName,
            //        Location = new Location
            //        {

            //            LocationID = d.Location!.LocationID,
            //            City = d.Location.City
            //        }
            //    }).ToListAsync();

        }

        public async Task<DepartmentDto> CreateDepartments(DepartmentCreateDto departmentDto)
        {
            var departmentEntity = new Departments
            {
                DepartmentName = departmentDto.DepartmentName,
                LocationId = departmentDto.LocationId,
                ManagerId = departmentDto.ManagerId
            };

            var existDep = await _context.Departments.CountAsync(d => d.DepartmentName == departmentDto.DepartmentName);

            if(existDep > 0)
            {
                throw new BusinessRuleException($"Ya existe un departamento con el nombre {departmentDto.DepartmentName}");
            }

            _context.Departments.Add(departmentEntity);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                DepartmentId = departmentEntity.DepartmentId,
                DepartmentName = departmentEntity.DepartmentName,
                LocationId = departmentEntity.LocationId,
                ManagerId = departmentEntity.ManagerId
            };
        }

        public async Task<bool> EliminarDepto(int id)
        {
            var depto = await _context.Departments.FindAsync(id);

            if(depto == null)
            {
                return false;
            }

            _context.Departments.Remove(depto);
           await _context.SaveChangesAsync();

            return true;

        }

        public async Task<bool> UpdateInfoDepto(int id,UpdateDepartmentDto updateDepartmentDto)
        {

            var deptoEntity = await _context.Departments.FindAsync(id);

            if(deptoEntity == null)
            {
                return false;
            }

            
            var nombre = updateDepartmentDto.DepartamentName;

            var existeOtroDepartamento = await _context.Departments.CountAsync(
                d => d.DepartmentId != id && d.DepartmentName == nombre);

            if (existeOtroDepartamento > 0)
            {
                throw new BusinessRuleException(
                    $"Ya existe un departamento con el nombre {nombre}.");
            }

            deptoEntity.DepartmentName = updateDepartmentDto.DepartamentName;
            deptoEntity.LocationId = updateDepartmentDto.LocationId ?? deptoEntity.LocationId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {

            return await _context.Departments
            .Where(e => e.DepartmentId == id)
            .Select(e => new DepartmentDto
            {
                DepartmentId = e.DepartmentId,
                DepartmentName = e.DepartmentName
            }).FirstOrDefaultAsync();

        }

        public async Task<bool> ChangeManager(int id, int manager_id)
        {
          var department =  await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return false;
            }

            var empleadoPerteneceAlDepartamento = await _context.Employees.CountAsync(e =>
             e.EmployeeId == manager_id && e.DepartmentId == id);

            if (empleadoPerteneceAlDepartamento == 0)
            {
                throw new BusinessRuleException(
                    $"El empleado {manager_id} no pertenece al departamento {id}.");
            }

            if (empleadoPerteneceAlDepartamento == 0)
            {
                return false;
            }

            if (department.ManagerId == manager_id)
            {
                return true;
            }


            department.ManagerId = manager_id;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
