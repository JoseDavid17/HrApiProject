using HrApiProject.Data;
using HrApiProject.Domain;
using HrApiProject.DTOs;
using HrApiProject.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Services
{
    public class JobServices : IJobService
    {
        private readonly HrDbContext _context;

        public JobServices(HrDbContext context)
        {
            _context = context;
        }

        public async Task<JobsDTO> createJob(JobCreateDTO jobCreateDTO)
        {
            var Existejob = await _context.Jobs.CountAsync(j=> j.JobTitle == jobCreateDTO.JobTitle);

            if (Existejob > 0)
            {
                throw new BusinessRuleException($"Ya existe un trabajo con el nombre {jobCreateDTO.JobTitle}");
            }

            var jobEntity = new Jobs
            {
                JobId = jobCreateDTO.JobId,
                JobTitle = jobCreateDTO.JobTitle,
                MinSalary = jobCreateDTO.MinSalary,
                MaxSalary = jobCreateDTO.MaxSalary
            };

            _context.Add(jobEntity);

            await _context.SaveChangesAsync();

            return new JobsDTO
            {
                JobId = jobEntity.JobId,
                JobTitle = jobEntity.JobTitle,
                MinSalary = jobEntity.MinSalary,
                MaxSalary = jobEntity.MaxSalary
            };

        }

        public async Task<bool> deleteJob(string id)
        {
            var jobEntity = await _context.Jobs.FindAsync(id);

            if(jobEntity == null)
            {
                return false;
            }

            _context.Remove(jobEntity);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<JobsDTO?> getJobId(string id)
        {
            return await _context.Jobs
            .Where(j => j.JobId == id)
            .Select(j => new JobsDTO
            {
                JobId = j.JobId,
                JobTitle = j.JobTitle,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary
            }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<JobsDTO>> getJobs(int pageNumber, int pageSize)
        {
            int resultadosPaginar = (pageNumber - 1) * pageSize;
            return await _context.Jobs
            .AsNoTracking()
            .OrderBy(j=>j.JobId)
            .Skip(resultadosPaginar)
            .Take(pageSize)
            .Select(j => new JobsDTO
            {
                JobId = j.JobId,
                JobTitle = j.JobTitle,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary
            }).ToListAsync();
      
        }

        public async Task<bool> updateJob(string id, UpdateJobsDTO updateJobsDTO)
        {
            var Existejob = await _context.Jobs.CountAsync(j => j.JobTitle == updateJobsDTO.JobTitle);

            if (Existejob > 0)
            {
                throw new BusinessRuleException($"Ya existe un trabajo con el nombre {updateJobsDTO.JobTitle}");
            }

            var jobEntity = await _context.Jobs.FindAsync(id);

            if (jobEntity == null)
            {
                return false;
            }

            jobEntity.JobTitle = updateJobsDTO.JobTitle;
            jobEntity.MinSalary = updateJobsDTO.MinSalary;
            jobEntity.MaxSalary = updateJobsDTO.MaxSalary;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
