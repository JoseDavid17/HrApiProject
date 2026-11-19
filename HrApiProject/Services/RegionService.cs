using HrApiProject.Data;
using HrApiProject.Domain;
using HrApiProject.DTOs;
using HrApiProject.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace HrApiProject.Services
{
    public class RegionService : IRegionService
    {

        private readonly HrDbContext _context;


        public RegionService(HrDbContext context)
        {
            _context = context;
        }

        public async Task<RegionDto> CreateRegion(RegionCreateDto regionDto)
        {
            var regionEntity = new Region
            {
                RegionName = regionDto.RegionName
            };

            _context.Regions.Add(regionEntity);
            await _context.SaveChangesAsync();

            return new RegionDto
            {
                Name = regionEntity.RegionName
            };

        }

        public async Task<bool> DeleteRegion(int id)
        {
            var regionEntity = await _context.Regions.FindAsync(id);

            if(regionEntity == null)
            {
                return false;
            }

            var paisesAsociados = await _context.Countries.CountAsync(r=>r.RegionId == id);

            if(paisesAsociados > 0)
            {
                throw new BusinessRuleException("No se puede eliminar la región porque tiene países asociados");
            }

            _context.Regions.Remove(regionEntity);
            await _context.SaveChangesAsync();

            return true;           

        }

        public async Task<RegionDto?> GetRegionById(int id)
        {
           return await _context.Regions.
                Where(r => r.RegionId == id)
                .Select(r => new RegionDto
                {
                    Id = r.RegionId,
                    Name = r.RegionName
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RegionDto>> GetRegions()
        {
            return await _context.Regions.
            Select(r => new RegionDto
            {
                Id = r.RegionId,
                Name = r.RegionName
            }).ToListAsync();


        }

        public async Task<bool> UpdateRegion(int id, RegionUpdateDto regionDto)
        {
            var regionEntity = await _context.Regions.FindAsync(id);
        
            if(regionEntity == null)
            {
                return false;
            }

            regionEntity.RegionName = regionDto.RegionName;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> CountRegions()
        {
            return await _context.Regions.CountAsync();
        }

    }
}
