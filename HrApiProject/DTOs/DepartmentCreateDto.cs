using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class DepartmentCreateDto
    {

        [Required(ErrorMessage = "El nombre del departamento es obligatorio", ErrorMessageResourceName = "Validación en departamentos")]
        [StringLength(30, ErrorMessage = "El nombre del departamento no debe superar los 30 caracteres.", ErrorMessageResourceName = "Validación en departamentos")]
        public string DepartmentName { get; set; } = null!;

        public int? ManagerId { get; set; } 
        public int? LocationId { get; set; }
    }
}
