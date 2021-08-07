using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM03500
    {
        public ADM03500()
        {
            ADM01100 = new HashSet<ADM01100>();
            TRN23100 = new HashSet<TRN23100>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClassId { get; set; }
        [Required]
        [Display(Name = "Classification")]
        [StringLength(60)]
        public string ClassName { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public virtual ICollection<ADM01100> ADM01100 { get; set; }
        public virtual ICollection<TRN23100> TRN23100 { get; set; }
    }
}