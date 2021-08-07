using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM16100
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PeriodID { get; set; }
        [Required]
        [Display(Name = "Week")]
        public int Week { get; set; }
        [Required]
        [Display(Name = "Period Start")]
        public DateTime PeriodStart { get; set; }
        [Required]
        [Display(Name = "Period End")]
        public DateTime PeriodEnd { get; set; }
        [Required]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }
        [Required]
        [Display(Name = "Pay Date")]
        public DateTime PayDate { get; set; }
        [Required]
        [Display(Name = "Period Year")]
        public int periodYear { get; set; }
    }
}