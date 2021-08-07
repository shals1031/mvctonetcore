using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM02200
    {
        public ADM02200()
        {
            ADM03300 = new HashSet<ADM03300>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecID { get; set; }
        [Required]
        [Display(Name = "Bank")]
        [StringLength(5)]
        public string BankId { get; set; }
        [Required]
        [Display(Name = "Branch Code")]
        [StringLength(11)]
        public string BranchId { get; set; }
        [Required]
        [Display(Name = "Branch Name")]
        [StringLength(50)]
        public string BranchName { get; set; }

        public virtual ADM02100 ADM02100 { get; set; }
        public virtual ICollection<ADM03300> ADM03300 { get; set; }
    }
}