using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class UpdateCountryDto
    {
        //[Required]
        //[StringLength(2, ErrorMessage = "El country debe de tener dos carácteres como minimo", ErrorMessageResourceName = "Validación countries")]
        //public string CountryId { get; set; } = null!;

        public string? CountryName { get; set; }

        public int? RegionId { get; set; }
    }
}
