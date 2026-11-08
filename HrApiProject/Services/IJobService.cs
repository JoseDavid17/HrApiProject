using HrApiProject.DTOs;

namespace HrApiProject.Services
{
    public interface IJobService
    {

        Task<IEnumerable<JobsDTO>> getJobs(int pageNumber, int pageSize);
        Task<JobsDTO?> getJobId(string id);
        Task<JobsDTO> createJob(JobCreateDTO jobCreateDTO);
        Task<bool> updateJob(string id, UpdateJobsDTO updateJobsDTO);
        Task<bool> deleteJob(string id);
            
    }
}
