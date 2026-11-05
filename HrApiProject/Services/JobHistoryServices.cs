using HrApiProject.Data;
using HrApiProject.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Services
{
    public class JobHistoryServices : IJobHistoryService
    {
        private readonly HrDbContext _context;

        public JobHistoryServices(HrDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobHistoryDto>> GetAllHistoryEmployee(int pagenumber, int pagesize)
        {

            int resultadosPaginar = (pagenumber - 1) * pagesize;

            return await _context.JobHistory
            .OrderBy(j => j.EmployeeId)
            .Skip(resultadosPaginar)
            .Take(pagesize)
            .Select(j => new JobHistoryDto
            {
                EmpId = j.EmployeeId,
                FullName = j.Employee.FirstName + " " + j.Employee.LastName,
                StartDate = j.StartDate,
                EndDate = j.EndDate,
                JobId = j.JobId,
                JobTitle = j.Jobs.JobTitle,
                DepartmentName = j.Departments!.DepartmentName
            }).ToListAsync();
        }

        public async Task<IEnumerable<dynamic>> GetAllHistory()
        {
            var blogs = await _context.JobHistory
            .FromSql($"SELECT * FROM job_history")
            .ToListAsync();

            return blogs;



        }

        public async Task<JobHistoryDto?> GetHistoryEmployeeById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
