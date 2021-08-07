using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TeliconLatest.DataEntities
{
    public partial class ADM13100
    {
        public ADM13100()
        {
            ADM01110 = new HashSet<ADM01110>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaterialID { get; set; }
        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string MaterialName { get; set; }
        [Required]
        [Display(Name = "Unit")]
        [StringLength(20)]
        public string MaterialUnit { get; set; }
        [Required]
        [Display(Name = "Code")]
        [Remote("MaterialCodeExists", "Admin", HttpMethod = "Post", ErrorMessage = "Material Code Already Exists", AdditionalFields = "initialID")]
        public string MaterialCode { get; set; }
        public decimal? MaxQty { get; set; }

        public virtual ICollection<ADM01110> ADM01110 { get; set; }
    }
}