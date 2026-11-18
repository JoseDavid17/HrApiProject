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

        [HttpGet("{id}")]

        public async Task<ActionResult<LocationDto>> ObtenerLocationId([FromRoute] int id)
        {
            var succes = await _locationService.GetLocationByid(id);

            if (succes == null)
            {
                return NotFound(new { message = $"No se encontró la location con el {id}" });
            }

            return Ok(succes);
        }

        [HttpPost]

        public async Task<ActionResult<LocationDto>> CrearLocation([FromBody] LocationCreateDto locationCreateDto)
        {
            var crearLocation = await _locationService.CreateLocation(locationCreateDto);

            return CreatedAtAction(nameof(ObtenerLocationId),new {Id = crearLocation.LocationID}, crearLocation);
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

        [HttpPut("{id}")]

        public async Task<ActionResult> EditarLocation([FromRoute] int id , [FromBody] LocationCreateDto locationCreateDto)
        {

            var succes = await _locationService.UpdateLocation(id, locationCreateDto);

            if (!succes)
            {
                return NotFound(new { message = $"No se encontró una location con el {id} para actualizarlo" });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> BorrarLocation([FromRoute] int id)
        {
            var succes = await _locationService.DeleteLocation(id);

            if (!succes)
            {
                return NotFound(new { message = $"No se encontró una location con el {id} para actualizarlo" });
            }

            return NoContent();
        }
    }
}
