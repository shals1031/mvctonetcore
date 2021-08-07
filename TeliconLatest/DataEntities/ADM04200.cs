using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM04200
    {
        public ADM04200()
        {
            ADM01100 = new HashSet<ADM01100>();
            ADM03300 = new HashSet<ADM03300>();
            ADM04210 = new HashSet<ADM04210>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentID { get; set; }
        [Required]
        [Display(Name = "Department Name")]
        [StringLength(35)]
        public string Name { get; set; }

        public virtual ICollection<ADM01100> ADM01100 { get; set; }
        public virtual ICollection<ADM03300> ADM03300 { get; set; }
        public virtual ICollection<ADM04210> ADM04210 { get; set; }
    }
}