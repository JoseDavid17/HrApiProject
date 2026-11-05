using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    }
}
