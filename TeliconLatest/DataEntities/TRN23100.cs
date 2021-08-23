using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN23100
    {
        public TRN23100()
        {
            ADM03400 = new HashSet<ADM03400>();
            TRN13110 = new HashSet<TRN13110>();
            TRN13120 = new HashSet<TRN13120>();
            TRN23110 = new HashSet<TRN23110>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Workid { get; set; }
        [Display(Name = "Reference #")]
        [StringLength(40)]
        [Required]
        public string Wo_ref { get; set; }
        [Required]
        [StringLength(300)]
        [Display(Name = "Title")]
        public string Wo_title { get; set; }
        [Display(Name = "Requested By")]
        [StringLength(300)]
        public string Requestby { get; set; }
        [Required]
        [Display(Name = "Request Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Requestdt { get; set; }
        [Display(Name = "Client")]
        public int Wo_client { get; set; }
        [Display(Name = "Management %")]
        public double Wo_split { get; set; }
        [Display(Name = "Contractor %")]
        public double Wo_split2 { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Dispatch Date")]
        public DateTime Dispatchdt { get; set; }
        [StringLength(2)]
        public string Status { get; set; }
        public bool Submitted { get; set; }
        public DateTime? DateSubmitted { get; set; }
        [Display(Name = "Classification")]
        [Required]
        [DefaultValue(0)]
        public int ClassId { get; set; }
        [Display(Name = "Splice Docs")]
        [Required]
        public bool SpliceDocs { get; set; }
        [StringLength(36)]
        public string CreateBy { get; set; }
        public int? LocationID { get; set; }
        [Required]
        [Display(Name = "Area")]
        public int AreaID { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? dateVerified { get; set; }
        public DateTime? RollbackDate { get; set; }
        [Display(Name = "PO #")]
        [StringLength(40)]
        public string PONum { get; set; }
        [StringLength(2)]
        public string tempValue { get; set; }
        [Display(Name = "Completion Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CompletionDt { get; set; }
        [Display(Name = "Department")]
        public int? DepartmentID { get; set; }

        public virtual ADM01400 ADM01400 { get; set; }
        public virtual ADM03500 ADM03500 { get; set; }
        public virtual Users Users { get; set; }
        public virtual ADM03200 ADM03200 { get; set; }
        public virtual ICollection<ADM03400> ADM03400 { get; set; }
        public virtual ICollection<TRN13110> TRN13110 { get; set; }
        public virtual ICollection<TRN13120> TRN13120 { get; set; }
        public virtual ICollection<TRN23110> TRN23110 { get; set; }
    }
}