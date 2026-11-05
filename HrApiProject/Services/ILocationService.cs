using HrApiProject.Domain;
using HrApiProject.DTOs;

namespace HrApiProject.Services
{
    public interface ILocationService
    {

        Task<IEnumerable<LocationDto>> GetLocations(int pageNumber, int pageSize);

        Task<IEnumerable<dynamic>> GetCityCountryByCountry();

        Task<IEnumerable<LocationDto>> GetLocationsCustum();


    }
}
