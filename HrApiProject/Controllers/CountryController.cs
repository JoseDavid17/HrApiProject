using Microsoft.AspNetCore.Mvc;
using HrApiProject.Services;
using HrApiProject.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace HrApiProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDto>>> getcontries([FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
        {

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var getCountries = await _countryService.getAllCountry(pageNumber,pageSize);

            //return await _context.Countries.Include(c=>c.Region).Where(c=> c.CountryName != null && c.CountryName.StartsWith("C")).ToListAsync();
            //return await _context.Countries.Include(c => c.Region).Where(c=> EF.Functions.Like(c.CountryName,"C%")).ToListAsync();

            return Ok(getCountries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<CountryDto>>> GetCountryById([FromRoute] string id)
        {
            var countrybyId = await _countryService.GetCountryById(id);

            if(countrybyId == null)
            {
                return NotFound(new { message = $"El país {id} no existe" }); 
            }

            return Ok(countrybyId);
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteCountry([FromRoute] string id)
        {
            var countryId = await _countryService.deleteCountry(id);

            if(!countryId)
            {
                return NotFound(new { message = $"El país {id} no existe " });
            }

            return NoContent();
        }

        [HttpPost]

        public async Task<ActionResult<CountryDto>> CreateCountry(CountryCreateDto countryDto)
        {
            var createCountries = await _countryService.createCountry(countryDto);

            return CreatedAtAction(nameof(GetCountryById), new { id=createCountries.CountryId }, createCountries);
        }

        [HttpPatch("{id}")]

        public async Task<ActionResult> UpdateCountry([FromRoute] string id, [FromBody] UpdateCountryDto updateCountryDto)
        {
            var updateCoutry = await _countryService.updateCountry(id,updateCountryDto);

            if (!updateCoutry)
            {
                return NotFound(new {message ="No se encontró dicho país"});
            }
            return NoContent();
        }

        [HttpGet("PaisesRegiones")]
        public async Task<ActionResult> Contar()
        {
            var contar = await _countryService.GetRegionsByCountryAsync();
            
            return Ok(contar);
        }

    }
}
