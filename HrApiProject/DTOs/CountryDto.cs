using HrApiProject.Domain;

namespace HrApiProject.DTOs
{
    public sealed class CountryDto
    {
        public string CountryId { get; set; } = null!;
        public string? CountryName { get; set; }
        public int? RegionId { get; set; }
        public RegionDto? Region { get; set; }
    }
}
