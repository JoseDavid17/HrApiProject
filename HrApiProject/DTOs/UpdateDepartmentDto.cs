using System.ComponentModel.DataAnnotations;

namespace HrApiProject.DTOs
{
    public sealed class UpdateDepartmentDto
    {
        //[Required(ErrorMessage = "El id del departamento debe ser proporcionado", ErrorMessageResourceName = " Validación en departamentos")]
        //public int DepartamentId { get; set; }

        public int ManagerId { get; set; }

        [Required(ErrorMessage = "El nombre del departamento debe ser proporcionado", ErrorMessageResourceName = " Validación en departamentos")]
        [StringLength(30, ErrorMessage = "El nombre del departamento no debe pasar los 30 carácteres", ErrorMessageResourceName = "Validación en departamentos")]
        public string DepartamentName { get; set; } = null!;
        //public int? ManagerId { get; set; }
        public int? LocationId { get; set; }
    }
}
