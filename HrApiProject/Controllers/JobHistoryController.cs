using HrApiProject.DTOs;
using HrApiProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace HrApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class JobHistoryController : ControllerBase
    {
        private readonly IJobHistoryService _jobHistoryService;

        public JobHistoryController(IJobHistoryService jobHistoryService)
        {
            _jobHistoryService = jobHistoryService;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<JobHistoryDto>>> GetJobHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {

            if(pageNumber < 1)
            {
                pageNumber = 1;
            }
            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var jobHistory = await _jobHistoryService.GetAllHistoryEmployee(pageNumber,pageSize);

            return Ok(jobHistory);
        }


        [HttpGet("jobss")]

        public async Task<ActionResult<IEnumerable<dynamic>>> Getjobs()
        {

            var jobHistory = await _jobHistoryService.GetAllHistory();

            return Ok(jobHistory);
        }


    }
}
