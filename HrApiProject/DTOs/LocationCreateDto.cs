using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class LocationCreateDto
    {
        [StringLength(40,ErrorMessage = "El campo no debe superar los 40 carácteres")]
        public string? StreetAddress { get; set; }
        [StringLength(12, ErrorMessage = "El campo no debe superar los 12 carácteres")]
        public string? PostalCode { get; set; }

        [StringLength(30, ErrorMessage = "El campo no debe superar los 30 carácteres")]
        public string City { get; set; } = null!;

        [StringLength(25, ErrorMessage = "El campo no debe superar los 25 carácteres")]
        public string? StateProvince { get; set; }

        [StringLength(2, ErrorMessage = "El campo no debe superar los 2 carácteres")]
        public string? CountryId { get; set; } 
    }
}
