using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class JobCreateDTO
    {
        [Required]
        [StringLength(10,ErrorMessage ="El campo no puede superar los 10 carácteres",ErrorMessageResourceName ="Validación en jobs")]
        public string JobId { get; set; } = null!;

        [Required]
        [StringLength(35, ErrorMessage = "El campo no puede superar los 35 carácteres", ErrorMessageResourceName = "Validación en jobs")]
        public string JobTitle { get; set; } = null!;
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
    }
}
