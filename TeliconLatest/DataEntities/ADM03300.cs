using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM03300
    {
        public ADM03300()
        {
            ADM03400 = new HashSet<ADM03400>();
            TRN04100 = new HashSet<TRN04100>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConID { get; set; }
        [Required]
        [Display(Name = "Employee ID")]
        [StringLength(4)]
        [Remote(action: "EmployeeIDExists", controller: "Contractor", ErrorMessage = "Employee ID Already Exists", AdditionalFields = "initialID", HttpMethod = "Post")]
        public string EmployeeID { get; set; }
        [Required]
        [Display(Name = "First Name")]
        [StringLength(30)]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(30)]
        public string LastName { get; set; }
        [StringLength(50)]
        public string Street { get; set; }
        [StringLength(30)]
        public string City { get; set; }
        [StringLength(30)]
        public string Parish { get; set; }
        [Display(Name = "Phone 1")]
        [StringLength(7)]
        public string Phone1 { get; set; }
        [Display(Name = "Phone 2")]
        [StringLength(7)]
        public string Phone2 { get; set; }
        [EmailAddress]
        [Required]
        [StringLength(30)]
        public string Email { get; set; }
        [StringLength(15)]
        public string TRN { get; set; }
        [Display(Name = "Birthday")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOB { get; set; }
        [Required]
        [Display(Name = "Level")]
        public int ConClass { get; set; }
        [Display(Name = "Emergency Contact")]
        [StringLength(30)]
        public string EmergencyCon { get; set; }
        [Display(Name = "Emergency Phone 1")]
        [StringLength(15)]
        public string EmerPhone1 { get; set; }
        [Display(Name = "Emergency Phone 2")]
        [StringLength(15)]
        public string EmerPhone2 { get; set; }
        [Display(Name = "Relation")]
        [StringLength(15)]
        public string EmerRelation { get; set; }
        [Display(Name = "Driver's License")]
        [StringLength(15)]
        public string DLicence { get; set; }
        [Display(Name = "Bank Account")]
        [StringLength(20)]
        public string Bankacc { get; set; }
        public int? Branch { get; set; }
        [Required]
        [Display(Name = "Department")]
        public int? DepartmentID { get; set; }
        [StringLength(7)]
        public string TechNo { get; set; }
        [Display(Name = "Plate No")]
        [StringLength(7)]
        public string PlateNo { get; set; }
        public int? AreaID { get; set; }
        public int? LocationID { get; set; }
        [Display(Name = "Pay Scale")]
        public int? payScale { get; set; }
        [Display(Name = "Active")]
        public bool? isActive { get; set; }
        [Display(Name = "Date of Engagement")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? EngagementDt { get; set; }
        [StringLength(15)]
        public string NIS { get; set; }

        public virtual ADM02200 ADM02200 { get; set; }
        public virtual ADM04200 ADM04200 { get; set; }
        public virtual ICollection<ADM03400> ADM03400 { get; set; }
        public virtual ICollection<TRN04100> TRN04100 { get; set; }
    }
}