using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class EmployeeCreateDto
    {

        [Required(ErrorMessage = "El Id del empleado es obligatorio", ErrorMessageResourceName = "Validación en empleados")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "El nombre del empleado es obligatorio.",ErrorMessageResourceName = "Validación en empleados")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "El apellido del empleado es obligatorio.", ErrorMessageResourceName = "Validación en empleados")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Formato de correo invalido", ErrorMessageResourceName = "Validación en empleados")]
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage ="La fecha del empleado es obligatoria.", ErrorMessageResourceName ="Validación en empleados")]
        public DateTime HireDate { get; set; } = DateTime.Now;
        public double? CommissionPct { get; set; }
        public double? Salary { get; set; }
        public int? DepartmentId { get; set; }

        [Required(ErrorMessage = "El código del puesto (JobId) es obligatorio")]
        public string JobId { get; set; } = null!;



    }
}
