using HrApiProject.DTOs;

namespace HrApiProject.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDetailsDto>> GetAllEmployees(int pagenumber, int pagesize);

        Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDetailsDto> CreateEmployeesAsync(EmployeeCreateDto employeeCreateDto);
        Task<bool> UpdateDatosEmployee(int id, UpdateDataEmpDTO updatedatosEmpleado);
        Task<bool> DeleteEmpleado(int id);
        Task<IEnumerable<EmployeeSupervisorsDto>> Supervisores(int pagenumber, int pagesize);

        Task<bool> ChangeSupervisor(int employeeid, int supervisor, int departmentid);

        Task<IEnumerable<EmployeeOrganizationNodeDto>> Organigrama(int? employeeid = null);
    }
}
