using HrApiProject.Domain;

namespace HrApiProject.DTOs
{
    public sealed class LocationDto
    {
        public int LocationID { get; set; }

        public string? StreetAddress { get; set; }
        public string? PostalCode { get; set; }
        public string City { get; set; } = null!;
        public string? StateProvince { get; set; }

        public Country? Country { get; set; }

    }
}
