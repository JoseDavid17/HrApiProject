using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrApiProject.Domain
{
    [Table("REGIONS")]
    public class Region
    {
        [Key]
        [Column("REGION_ID")]
        public int RegionId { get; set; }

        [Column("REGION_NAME")]
        public string? RegionName { get; set; }
    }
}
