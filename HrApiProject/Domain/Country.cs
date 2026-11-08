using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrApiProject.Domain
{
    [Table("COUNTRIES")]
    public class Country
    {

        [Key]
        [Column("COUNTRY_ID")]
        public string CountryId { get; set; } = null!;

        [Column("COUNTRY_NAME")]
        public string? CountryName { get; set; }

        [Column("REGION_ID")]
        public int? RegionId { get; set; }

        [ForeignKey("RegionId")]
        public Region? Region { get; set; }

    }
}
