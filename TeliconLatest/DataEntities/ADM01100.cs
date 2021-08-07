using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM01100
    {
        public ADM01100()
        {
            ADM01110 = new HashSet<ADM01110>();
            ADM01150 = new HashSet<ADM01150>();
            ADM01250 = new HashSet<ADM01250>();
            ADM04210 = new HashSet<ADM04210>();
            TRN19110 = new HashSet<TRN19110>();
            TRN23110 = new HashSet<TRN23110>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RateID { get; set; }
        [Required]
        [Display(Name = "Description")]
        [StringLength(150)]
        public string RateDescr { get; set; }
        [Required]
        [StringLength(2)]
        [Display(Name = "Unit")]
        public string RateUnit { get; set; }
        [Display(Name = "Class")]
        public int RateClass { get; set; }
        public bool Active { get; set; }
        [Required]
        [Display(Name = "Client")]
        public int ClientID { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Required]
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        public decimal? MaxQty { get; set; }
        [Display(Name = "Has Materials")]
        public bool HasMaterials { get; set; }
        [Display(Name = "Materials Required")]
        public bool MaterialsRequired { get; set; }
        [Display(Name = "Alt Code")]
        [StringLength(5)]
        public string AltCode { get; set; }

        public virtual ADM04200 ADM04200 { get; set; }
        public virtual ADM03500 ADM03500 { get; set; }
        public virtual ICollection<ADM01110> ADM01110 { get; set; }
        public virtual ICollection<ADM01150> ADM01150 { get; set; }
        public virtual ICollection<ADM01250> ADM01250 { get; set; }
        public virtual ICollection<ADM04210> ADM04210 { get; set; }
        public virtual ICollection<TRN19110> TRN19110 { get; set; }
        public virtual ICollection<TRN23110> TRN23110 { get; set; }
    }
}