using HrApiProject.Domain;

namespace HrApiProject.DTOs
{
    public sealed class DepartmentDto
    {
      
        public  int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = null!;

        public int? ManagerId { get; set; }

        public int? LocationId { get; set; }

        public LocationDto? Location { get; set; }
    }
}
