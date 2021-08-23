using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM04100
    {
        public ADM04100()
        {
            TRN04100 = new HashSet<TRN04100>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeductionID { get; set; }
        [Required]
        [Display(Name = "Deduction Name")]
        [StringLength(70)]
        public string Name { get; set; }

        public virtual ICollection<TRN04100> TRN04100 { get; set; }
    }
}
