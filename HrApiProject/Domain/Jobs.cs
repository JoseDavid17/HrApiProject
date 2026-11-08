using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Domain
{
    [Table("JOBS")]
    public class Jobs
    {
        [Key]
        [Column("JOB_ID")]
        public string JobId { get; set; } = null!;

        [Column("JOB_TITLE")]
        public string JobTitle { get; set; } = null!;

        [Column("MIN_SALARY")]
        [Precision(6, 0)]
        public decimal? MinSalary { get; set; }

        [Column("MAX_SALARY")]
        [Precision(6, 0)]
        public decimal? MaxSalary { get; set; }

    }
}
