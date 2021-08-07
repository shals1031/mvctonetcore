using System;
using System.ComponentModel.DataAnnotations;

namespace TeliconLatest.Models
{
    [MetadataTypeAttribute(typeof(TRN04100MetaData))]
    public partial class TRN04100Meta
    {
        public class TRN04100MetaData
        {
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
            public DateTime EndDate { get; set; }
            [Display(Name = "Hold")]
            public bool HoldIt { get; set; }
            public string DeductionName { get; set; }
            public int Periods { get; set; }
            public double YearToDateAmount { get; set; }
        }
    }
}