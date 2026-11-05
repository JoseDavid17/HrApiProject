using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrApiProject.Domain
{
    [Table("DEPARTMENTS")]
    public class Departments
    {
        [Key]
        [Column("DEPARTMENT_ID")]
        public int DepartmentId { get; set; }

        [Column("DEPARTMENT_NAME")]
        public string DepartmentName { get; set; } = null!;

        [Column("MANAGER_ID")]
        public int? ManagerId { get; set; }

        [Column("LOCATION_ID")]
        public int? LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location? Location { get; set; }

    }
}
