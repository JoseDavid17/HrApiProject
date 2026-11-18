using HrApiProject.DTOs;
using HrApiProject.Data;
using HrApiProject.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HrApiProject.Exceptions;

namespace HrApiProject.Services
{
    public class LocationService : ILocationService
    {
        private readonly HrDbContext _context;

        public LocationService(HrDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<LocationDto>> GetLocations(int pageNumber, int pageSize)
        {

            int registroaSaltar = (pageNumber - 1) * pageSize;

            return await _context.Locations
              .OrderBy(l=>l.LocationID)
              .Skip(registroaSaltar)
              .Take(pageSize)
             .Select(l => new LocationDto
             {
                 LocationID = l.LocationID,
                 City = l.City,
                 StateProvince = l.StateProvince,
                 StreetAddress = l.StreetAddress,
                 PostalCode = l.PostalCode,
                 CountryId = l.CountryId,

             }).ToListAsync();

            //return await _context.Locations
            //   .Include(l=> l.Country).ToListAsync(); 

        }

        public async Task<IEnumerable<dynamic>> GetCityCountryByCountry()
        {
           return await _context.Locations
                .GroupBy(l=> l.CountryId)
                .Select(g=> new 
                {
                    CountryId = g.Key,
                    CityCount = g.Count(l => l.City != null)
                }).ToListAsync();

        }

        public async Task<IEnumerable<LocationDto>> GetLocationsCustum()
        {
            return await _context.Locations
            .Select(l => new LocationDto
            {

                LocationID = l.LocationID,
                City = l.City,
                StreetAddress = l.StreetAddress,

                //Country = l.Country!.CountryName,
                //CountryCode = l.Country.CountryId

            }).ToListAsync();

        }

        public async Task<LocationDto> CreateLocation(LocationCreateDto locationCreateDto)
        {
            if (locationCreateDto.CountryId is not null)
            {
                var existePais = await _context.Countries
                    .CountAsync(c =>
                        c.CountryId == locationCreateDto.CountryId);

                if (existePais == 0)
                {
                    throw new BusinessRuleException(
                        "El país seleccionado no existe.");
                }
            }

            var location = new Location
            {
                StreetAddress = locationCreateDto.StreetAddress,
                PostalCode = locationCreateDto.PostalCode,
                City = locationCreateDto.City,
                StateProvince = locationCreateDto.StateProvince,
                CountryId = locationCreateDto.CountryId
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return new LocationDto
            {
                LocationID = location.LocationID,
                StreetAddress = location.StreetAddress,
                PostalCode = location.PostalCode,
                City = location.City,
                StateProvince = location.StateProvince,
                CountryId = location.CountryId
            };
        }

        public async Task<bool> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);

            if(location == null)
            {
                return false;
            }

            _context.Remove(location);

            await _context.SaveChangesAsync();

            return true;    
        }

        public async Task<bool> UpdateLocation(int id, LocationCreateDto locationCreateDto)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location is null)
            {
                return false;
            }

            if (locationCreateDto.CountryId is not null)
            {
                var existePais = await _context.Countries
                    .CountAsync(c =>
                        c.CountryId == locationCreateDto.CountryId);

                if (existePais == 0)
                {
                    throw new BusinessRuleException(
                        "El país seleccionado no existe.");
                }
            }

            location.StreetAddress = locationCreateDto.StreetAddress;
            location.PostalCode = locationCreateDto.PostalCode;
            location.City = locationCreateDto.City;
            location.StateProvince = locationCreateDto.StateProvince;
            location.CountryId = locationCreateDto.CountryId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LocationDto?> GetLocationByid(int id)
        {

            return await _context.Locations
            .Where(l => l.LocationID == id)
            .Select(l => new LocationDto
            {
                LocationID = l.LocationID,
                City = l.City,
                PostalCode = l.PostalCode,
                StateProvince = l.StateProvince,
                StreetAddress = l.StreetAddress,                
            }).FirstOrDefaultAsync();
        }
    }
}
