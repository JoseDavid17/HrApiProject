using HrApiProject.Domain;
using HrApiProject.DTOs;

namespace HrApiProject.Services
{
    public interface IDepartmentService 
    {
        Task<IEnumerable<DepartmentDto>> GetDepartments(int pageNumber, int pageSize);
        Task<DepartmentDto> CreateDepartments(DepartmentCreateDto departmentDto);

        Task<IEnumerable<Departments>> GetDepartmentss();


        Task<bool> ChangeManager(int id, int manager_id);

        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);


        Task<bool> EliminarDepto(int id);

        Task<bool> UpdateInfoDepto(int id,UpdateDepartmentDto updateDepartmentDto);
    }
}
