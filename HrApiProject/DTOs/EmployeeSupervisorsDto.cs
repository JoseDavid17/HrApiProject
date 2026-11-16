namespace HrApiProject.DTOs
{
    public sealed class EmployeeSupervisorsDto
    {
        public int EmployeeId { get; set; }
        public int? DepartmentId { get; set; }

        public int? SupervisorId { get; set; }
        public int? SupervisorDepartmentId { get; set; }
        public string? SupervisorDepartmentName { get; set; }

        public string? FullEmployeeName { get; set; }
        public string FullSupervisorName { get; set; } = null!;

        public string DepartmentName { get; set; } = null!;
    }
}
