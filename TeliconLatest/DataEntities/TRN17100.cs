using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN17100
    {
        public TRN17100()
        {
            TRN17110 = new HashSet<TRN17110>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuotationId { get; set; }
        [Display(Name = "Reference #")]
        [StringLength(20)]
        [Required]
        public string Quot_ref { get; set; }
        [Required]
        [StringLength(150)]
        [Display(Name = "Title")]
        public string Quot_title { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Requested By")]
        public string Requestby { get; set; }
        [Required]
        [Display(Name = "Request Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Requestdt { get; set; }
        [StringLength(75)]
        [Display(Name = "Street")]
        public string RequestStreet { get; set; }
        [StringLength(75)]
        [Display(Name = "City")]
        public string RequestCity { get; set; }
        [StringLength(75)]
        [Display(Name = "Country")]
        public string RequestCountry { get; set; }
        [StringLength(36)]
        public string CreateBy { get; set; }

        public virtual Users Users { get; set; }
        public virtual ICollection<TRN17110> TRN17110 { get; set; }
    }
}