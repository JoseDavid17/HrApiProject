namespace HrApiProject.DTOs
{
    public sealed class EmployeeSupervisorsDto
    {
        public string? FullEmployeeName { get; set; }
        public string FullSupervisorName { get; set; } = null!;

        public string DepartmentName { get; set; } = null!;
    }
}
