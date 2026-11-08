using HrApiProject.Data;
using HrApiProject.Domain;
using HrApiProject.DTOs;
using HrApiProject.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Services
{
    public class CountryService : ICountryService
    {
        private readonly HrDbContext _context;

        public  CountryService(HrDbContext context)
        {
            _context = context;
        }

        public async Task<CountryDto> createCountry(CountryCreateDto countryCreateDto)
        {
            var existeCountry = await _context.Countries
                .CountAsync(c => c.CountryName == countryCreateDto.CountryName && c.CountryId ==countryCreateDto.CountryId);

            if (existeCountry > 0)
            {
                throw new BusinessRuleException(
                    $"El país {countryCreateDto.CountryName} ya está registrado");
            }

            var countryEntity = new Country
            {
                CountryId = countryCreateDto.CountryId,
                CountryName = countryCreateDto.CountryName,
                RegionId = countryCreateDto.RegionId
            };

            _context.Countries.Add(countryEntity);
            await _context.SaveChangesAsync();

            //return new CountryDto
            //{
            //    CountryId = countryEntity.CountryId,
            //    CountryName = countryEntity.CountryName,
            //    RegionId = countryEntity.RegionId
            //};

            return (await GetCountryById(countryEntity.CountryId))!;
        }

        public async Task<bool> deleteCountry(string CountryId)
        {
            var deleteCountry = await _context.Countries.FindAsync(CountryId);

            if (deleteCountry == null)
            {
                return false;
            }

            _context.Remove(deleteCountry);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CountryDto>> getAllCountry(int pageNumber, int pageSize)
        {
            int resultadosApaginar = (pageNumber - 1) * pageSize;

            return await _context.Countries
              .AsNoTracking()
              .OrderBy(c => c.CountryId)
              .Skip(resultadosApaginar)
              .Take(pageSize)
              .Select(c => new CountryDto
              {
                  CountryId = c.CountryId,
                  CountryName = c.CountryName,
                  Region = new RegionDto
                  {
                      Name = c.Region!.RegionName
                  }
              }).ToListAsync();
        }

        public async Task<CountryDto?> GetCountryById(string id)
        {
            return await _context.Countries
            .Where(c => c.CountryId == id)
            .Select(c => new CountryDto
            {
                CountryId=c.CountryId,
                CountryName=c.CountryName,
                Region = new RegionDto
                {
                    Name = c.Region!.RegionName
                }

            }).FirstOrDefaultAsync();
        }

        public async Task<bool> updateCountry(string id, UpdateCountryDto countryUpdateDto)
        {
            var existeCountry = await _context.Countries
                .CountAsync(c => c.CountryName == countryUpdateDto.CountryName);

            if (existeCountry > 0)
            {
                throw new BusinessRuleException(
                    $"El país {countryUpdateDto.CountryName} ya está registrado");
            }

            var country = await _context.Countries.FindAsync(id);

            if(country == null)
            {
                return false;
            }

            country.CountryName = countryUpdateDto.CountryName ?? country.CountryName;
            country.RegionId = countryUpdateDto.RegionId ?? country.RegionId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Dictionary<string,int>> GetRegionsByCountryAsync()
        {
            return await _context.Countries
            .GroupBy(c => c.Region!.RegionName!)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
    }
}
