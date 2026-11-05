using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrApiProject.Domain
{
    [Table("EMPLOYEES")]
    public class Employees
    {
        [Key]
        [Column("EMPLOYEE_ID")]
        public int EmployeeId { get; set; }

        [Column("FIRST_NAME")]
        public string? FirstName  { get; set; }

        [Column("LAST_NAME")]
        public string LastName { get; set; } = null!;

        [Column("EMAIL")]
        public string Email { get; set; } = null!;

        [Column("PHONE_NUMBER")]
        public string? Phone { get; set; }

        [Column("HIRE_DATE")]
        public DateTime Hiredate { get; set; }

        [Column("JOB_ID")]
        public string JobId { get; set; } = null!;

        [Column("SALARY")]
        public double? Salary { get; set; }

        [Column("COMMISSION_PCT")]
        public double? CommissionPct { get; set; }

        [Column("MANAGER_ID")]
        public int? ManagerId { get; set; }

        [Column("DEPARTMENT_ID")]
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]

        public Departments? Department { get; set; }

        [ForeignKey("JobId")]
        public Jobs? Jobs { get; set; } 

    }
}
