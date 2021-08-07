using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM22100
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VehicleID { get; set; }
        [Required]
        [Display(Name = "Plate No")]
        [StringLength(7)]
        public string PlateNo { get; set; }
        [Required]
        [Display(Name = "Fleet No")]
        [StringLength(7)]
        public string FleetNo { get; set; }
        [Required]
        [Display(Name = "Owned By")]
        public int? OwnedBy { get; set; }
        [Required]
        [Display(Name = "Contractor")]
        public int? ConID { get; set; }
    }
}
