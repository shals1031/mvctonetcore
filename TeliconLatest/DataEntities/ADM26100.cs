using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM26100
    {
        public ADM26100()
        {
            ADM01400 = new HashSet<ADM01400>();
            TRN19100 = new HashSet<TRN19100>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ZoneID { get; set; }
        [Required]
        [Display(Name = "Zone Name")]
        [StringLength(150)]
        public string Name { get; set; }
        [Display(Name = "Supervisor")]
        [StringLength(500)]
        public string SupervisorName { get; set; }
        [Display(Name = "Address")]
        [StringLength(500)]
        public string Addr { get; set; }
        [Display(Name = "Email")]
        [StringLength(300)]
        public string Email { get; set; }
        [Display(Name = "Phone")]
        [StringLength(40)]
        public string Phone { get; set; }
        [Display(Name = "Place")]
        [StringLength(200)]
        public string Place { get; set; }

        public virtual ICollection<ADM01400> ADM01400 { get; set; }
        public virtual ICollection<TRN19100> TRN19100 { get; set; }
    }
}