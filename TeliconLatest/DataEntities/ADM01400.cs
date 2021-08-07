using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM01400
    {
        public ADM01400()
        {
            TRN23100 = new HashSet<TRN23100>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int areaID { get; set; }
        [Required]
        [Display(Name = "Zone")]
        public int zoneID { get; set; }
        [Required]
        [Display(Name = "Area Name")]
        [StringLength(150)]
        public string areaName { get; set; }

        public virtual ADM26100 ADM26100 { get; set; }
        public virtual ICollection<TRN23100> TRN23100 { get; set; }
    }
}