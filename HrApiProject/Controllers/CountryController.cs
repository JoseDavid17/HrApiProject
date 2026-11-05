using HrApiProject.Domain;
using HrApiProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace HrApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class CountryController : ControllerBase
    {
        private readonly HrDbContext _context;

        public CountryController(HrDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {

            //return await _context.Countries.Include(c=>c.Region).Where(c=> c.CountryName != null && c.CountryName.StartsWith("C")).ToListAsync();
            //return await _context.Countries.Include(c => c.Region).Where(c=> EF.Functions.Like(c.CountryName,"C%")).ToListAsync();
            return await _context.Countries.Include(c => c.Region).ToListAsync();

        }

        [HttpGet("PaisesRegiones")]
        public async Task<IActionResult> GetRegionsByCountry()
        {

            //return await _context.Countries.Include(c=>c.Region).Where(c=> c.CountryName != null && c.CountryName.StartsWith("C")).ToListAsync();
            //return await _context.Countries.Include(c => c.Region).Where(c=> EF.Functions.Like(c.CountryName,"C%")).ToListAsync();

            var result = await _context.Countries
            .GroupBy(c => c.Region!.RegionName)
            .Select(g => new
            {
                RegionName = g.Key,
                TotalPaises = g.Count()
            }).ToListAsync();

            return Ok(result);
        }

    }
}
