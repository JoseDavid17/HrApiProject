using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public class UpdateDataEmpDTO
    {
        [StringLength(20, ErrorMessage = "El nombre del empleado no puede superar los 20 carácteres.", ErrorMessageResourceName = "Validación en empleados")]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "El apellido del empleado no puede superar los 25 carácteres", ErrorMessageResourceName = "Validación en empleados")]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(25, ErrorMessage = "El correo del empleado no puede superar los 25 carácteres", ErrorMessageResourceName = "Validación en empleados")]
        public string Email { get; set; } = null!;

        [StringLength(20, ErrorMessage = "El telefono del empleado no puede superar los 20 carácteres.", ErrorMessageResourceName = "Validación en empleados")]
        public string? PhoneNumber { get; set; }
        public double? Salary { get; set; }
        public double? CommissionPct { get; set; }
    }
}
