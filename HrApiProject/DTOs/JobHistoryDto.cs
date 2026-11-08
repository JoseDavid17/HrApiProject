namespace HrApiProject.DTOs
{
    public sealed class JobHistoryDto
    {
        public int EmpId { get; set; }

        public string FullName { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string JobId { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public string? DepartmentName { get; set; }

    }
}
