using HrApiProject.DTOs;
namespace HrApiProject.Services
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionDto>> GetRegions();
        Task<RegionDto> CreateRegion(RegionCreateDto regionDto);
        Task<RegionDto?> GetRegionById(int id);

        Task<bool> UpdateRegion(int id, RegionUpdateDto regionDto);

        Task<bool> DeleteRegion(int id);

        Task<int> CountRegions();

    }
}
