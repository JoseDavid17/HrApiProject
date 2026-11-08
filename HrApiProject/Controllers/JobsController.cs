using HrApiProject.DTOs;
using HrApiProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace HrApiProject.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {

        private readonly IJobService _jobservice;

        public JobsController(IJobService jobService )
        {
            _jobservice = jobService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobsDTO>>> GetJobs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize =10)
        {
            if(pageNumber < 1)
            {
                pageNumber = 1;
            }
            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var jobs = await _jobservice.getJobs(pageNumber, pageSize);

            return Ok(jobs); 
        }

        [HttpPost]
        public async Task<ActionResult<JobsDTO>> CreateJobs([FromBody] JobCreateDTO jobCreateDTO)  {

            var creatJob = await _jobservice.createJob(jobCreateDTO);

            return CreatedAtAction(nameof(GetJobById), new {id = creatJob.JobId}, creatJob );
        }

        [HttpGet("{id}")]

        public async Task<ActionResult> GetJobById([FromRoute] string id)
        {
            var job = await _jobservice.getJobId(id);

            if(job == null)
            {
                return NotFound(new { message = $"No se encontró el trabajo con el {id}"});
            }

            return Ok(job);

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJobID([FromRoute] string id)
        {
            var job = await _jobservice.deleteJob(id);

            if(!job)
            {
                return NotFound(new { message = $"No se encontró un job para {id}" });
            }

            return NoContent();
        }

        [HttpPut]

        public async Task<ActionResult> UpdateJob([FromRoute] string id, [FromBody] UpdateJobsDTO updateJobsDTO)
        {
            var job = _jobservice.updateJob(id, updateJobsDTO);

            if( job == null)
            {
                return NotFound(new { message = $"No se encontró un job para {id}" });
            }

            return NoContent();

        }      

    }
}
