using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrApiProject.Domain
{
  [Table("LOCATIONS")]

    public class Location 
    {
        [Key]
        [Column("LOCATION_ID")]
        public int LocationID { get; set; }

        [Column("STREET_ADDRESS")]
        public string? StreetAddress { get; set; }

        [Column("POSTAL_CODE")]
        public string? PostalCode { get; set; }

        [Column("CITY")]
        public required string City { get; set; } 

        [Column("STATE_PROVINCE")]
        public string? StateProvince { get; set; }

        [Column("COUNTRY_ID")]
        public string? CountryId { get; set; }

        [ForeignKey("CountryId")]
        public Country? Country { get; set; }

    }
}
