using HrApiProject.DTOs;

namespace HrApiProject.Services
{
    public interface ICountryService
    {
        Task<IEnumerable<CountryDto>> getAllCountry(int pageNumber, int pageSize);
        Task<CountryDto> createCountry(CountryCreateDto countryCreateDto);
        Task<bool> updateCountry(string id, UpdateCountryDto countryUpdateDto);
        Task<CountryDto?> GetCountryById(string id);
        Task<bool> deleteCountry(string CountryId);
        Task<Dictionary<string, int>> GetRegionsByCountryAsync();
    }
}
