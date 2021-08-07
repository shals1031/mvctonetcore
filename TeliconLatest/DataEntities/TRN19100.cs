using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN19100
    {
        public TRN19100()
        {
            TRN19110 = new HashSet<TRN19110>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SInvoiceId { get; set; }
        [Display(Name = "Invoice #")]
        [StringLength(20)]
        [Required]
        public string SInv_ref { get; set; }
        [Required]
        [StringLength(150)]
        [Display(Name = "Title")]
        public string SInv_title { get; set; }
        [Required]
        [Display(Name = "Zone")]
        public int ZoneId { get; set; }
        [Required]
        [Display(Name = "Request Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Requestdt { get; set; }
        [StringLength(36)]
        public string CreateBy { get; set; }

        public virtual Users Users { get; set; }
        public virtual ADM26100 ADM26100 { get; set; }
        public virtual ICollection<TRN19110> TRN19110 { get; set; }
    }
}