using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM16200
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int POID { get; set; }
        [Required]
        [Display(Name = "PO Number")]
        [StringLength(25)]
        public string PONUM { get; set; }
        [Required]
        [Display(Name = "PO Total")]
        public decimal TOTAL { get; set; }
        [Display(Name = "PO Balance")]
        public decimal? BALANCE { get; set; }
        [Display(Name = "Description")]
        [StringLength(250)]
        public string Description { get; set; }
        [Display(Name = "Closed")]
        public bool IsClosed { get; set; }
        [Required]
        [Display(Name = "PO Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PODate { get; set; }
    }
}