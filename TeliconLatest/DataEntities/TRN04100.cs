using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN04100
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeductionConductorID { get; set; }
        public int ConductorID { get; set; }
        [Required]
        [Display(Name = "Deduction")]
        public int DeductionID { get; set; }
        [Display(Name = "Recurring")]
        public bool Recurring { get; set; }
        [Display(Name = "Amount")]
        public double Amount { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Hold")]
        public bool HoldIt { get; set; }
        public double YTDAmount { get; set; }

        public virtual ADM03300 ADM03300 { get; set; }
        public virtual ADM04100 ADM04100 { get; set; }
    }
}