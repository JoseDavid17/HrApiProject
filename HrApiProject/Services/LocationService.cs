using HrApiProject.DTOs;
using HrApiProject.Data;
using HrApiProject.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

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
                 PostalCode = l.PostalCode
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
    }
}
