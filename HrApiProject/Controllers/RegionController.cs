using HrApiProject.Data;
using HrApiProject.Services;
using HrApiProject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;


        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet()]

        // entidad cruda proveniente de la base de datos sin Dtos
        //public async Task<ActionResult<IEnumerable<Region>>> GetRegions()
        //{
        //    return await _context.Regions.ToListAsync();
        //}

        public async Task<ActionResult<IEnumerable<RegionDto>>> GetRegions()
        {
            var regions = await _regionService.GetRegions();

            return Ok(regions);
        }

        [HttpGet("ContarRegiones")]

        public async Task<ActionResult<int>> CountRegions()
        {
            var regions = await _regionService.CountRegions();

            return Ok(regions);
        }

        [HttpPost]
        public async Task<ActionResult<RegionDto>> CreateRegion(RegionCreateDto regionDto)
        {
            var createdRegion = await _regionService.CreateRegion(regionDto);

            return CreatedAtAction(nameof(GetRegionById), new {id = createdRegion.Id}, createdRegion);

        }

        [HttpGet("{id}")]   
        public async Task<ActionResult<RegionDto>> GetRegionById(int id)
        {
          var region = await _regionService.GetRegionById(id);

            if(region == null)
            {
                return NotFound(new { message = $"No se encontró la región para el ID {id}." });
            }

            return Ok(region);
        }

        [HttpPut("{id}")]

        public async Task<ActionResult> UpdateRegion([FromRoute] int id, [FromBody] RegionUpdateDto regionDto)
        {
            var succes = await _regionService.UpdateRegion(id, regionDto);

            if (!succes)
            {
                return NotFound(new { message = $"No se encontró la región para el ID {id}." });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteRegion([FromRoute] int id)
        {
            var succes = await _regionService.DeleteRegion(id);

            if(!succes)
            {
                return NotFound(new { message = $"No se encontró la región para el ID {id}." });
            }

            return NoContent();
        }
    }
}
