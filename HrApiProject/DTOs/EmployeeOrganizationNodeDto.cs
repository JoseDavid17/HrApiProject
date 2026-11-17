namespace HrApiProject.DTOs
{
    public sealed class EmployeeOrganizationNodeDto
    {
        public int EmployeeId { get; set; }
        public int? ManagerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public List<EmployeeOrganizationNodeDto> Children { get; set; } = new();
    }
}
