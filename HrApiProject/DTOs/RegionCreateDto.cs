using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class RegionCreateDto
    {

        public int RegionId { get; set; }

        [Required(ErrorMessage ="El nombre de la región es obligatorio", ErrorMessageResourceName = "Validación en regiones")]
        [StringLength(25, ErrorMessage = "El nombre de la región no puede superar los 25 carácteres.", ErrorMessageResourceName ="Validación en regiones")]
        public string RegionName { get; set; } = null!;


    }
}
