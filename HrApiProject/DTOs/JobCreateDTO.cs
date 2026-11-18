using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class JobCreateDTO
    {
        [Required]
        [StringLength(10, ErrorMessage = "El código no puede superar los 10 caracteres")]
        public string JobId { get; set; } = null!;

        [Required]
        [StringLength(35, ErrorMessage = "El nombre no puede superar los 35 caracteres", ErrorMessageResourceName = "Validación en jobs")]
        public string JobTitle { get; set; } = null!;
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
    }
}
