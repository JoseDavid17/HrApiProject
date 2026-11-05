using HrApiProject.DTOs;

namespace HrApiProject.Services
{
    public interface IJobHistoryService
    {
        Task<IEnumerable<JobHistoryDto>> GetAllHistoryEmployee(int pagenumber, int pagesize);

        Task<IEnumerable<dynamic>> GetAllHistory();

        Task<JobHistoryDto?> GetHistoryEmployeeById(int id);
        //Task<JobHistoryDto> CreateHistoryEmployee(EmployeeCreateDto employeeCreateDto);
    }
}
