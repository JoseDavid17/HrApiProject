using HrApiProject.Domain;
using HrApiProject.DTOs;

namespace HrApiProject.Services
{
    public interface ILocationService
    {

        Task<IEnumerable<LocationDto>> GetLocations(int pageNumber, int pageSize);

        Task<LocationDto?> GetLocationByid(int id);

        Task<LocationDto> CreateLocation(LocationCreateDto locationCreateDto);

        Task<IEnumerable<dynamic>> GetCityCountryByCountry();

        Task<IEnumerable<LocationDto>> GetLocationsCustum();

        Task<bool> DeleteLocation(int id);

        Task<bool> UpdateLocation(int id, LocationCreateDto locationCreateDto);


    }
}
