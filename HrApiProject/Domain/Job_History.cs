using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrApiProject.Domain
{
    [Table("JOB_HISTORY")]
    public class Job_History
    {
        [Key]
        [Column("EMPLOYEE_ID")]
        public int EmployeeId { get; set; }

        [Column("START_DATE")]
        public DateTime StartDate { get; set; }

        [Column("END_DATE")]
        public DateTime EndDate { get; set; }

        [Column("JOB_ID")]
        public string JobId { get; set; } = null!;

        [Column("DEPARTMENT_ID")]
        public int? DepartmentId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employees Employee { get; set; } = null!;

        [ForeignKey("JobId")]
        public Jobs Jobs { get; set; } = null!;

        [ForeignKey("DepartmentId")]
        public Departments? Departments { get; set; }
    }
}
