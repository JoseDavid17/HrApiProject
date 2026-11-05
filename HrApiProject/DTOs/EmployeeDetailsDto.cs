using HrApiProject.Domain;

namespace HrApiProject.DTOs
{
    public sealed class EmployeeDetailsDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public DateTime HireDate {  get; set; }
        public double? Salary { get; set; }

        public double? CommissionPct { get; set; }

        public string? DepartmentName { get; set; }

        public string? JobTitle { get; set; }


        // podes acceder a otro dto cuando no tenes declarada en las propiedades, la propiedad a la cual ocupas acceder si tenes la propiedad declarada en la clase no hay problema.
        //public DepartmentDto? Department { get; set; }


    }
}
