using Microsoft.AspNetCore.Mvc;
using HrApiProject.Services;
using HrApiProject.DTOs;

namespace HrApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  
    public class LocationsController : ControllerBase
    {
       private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {

            if(pageNumber < 0)
            {
                pageNumber = 1;
            }
            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var locations = await _locationService.GetLocations(pageNumber,pageSize);

            return Ok(locations);
        }

        [HttpGet("ContarPaisesCiudad")]

        public async Task<ActionResult<IEnumerable<dynamic>>> GetCityCountryByCountry()
        {
            var result = await _locationService.GetCityCountryByCountry();

            return Ok(result);
        }

        [HttpGet("countries-locaciones")]

        public async Task<IActionResult> GetLocationsCustum()
        {

            var locations = _locationService.GetLocationsCustum();

            return Ok(locations);

        }

    }
}
