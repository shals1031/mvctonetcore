using System;
using System.ComponentModel.DataAnnotations;

namespace TeliconLatest.Models
{
    public class CustomActivityModel
    {
        public int RateID { get; set; }

        [Required(ErrorMessage = "Description required.")]
        public string RateDescr { get; set; }

        [Required(ErrorMessage = "Please select rate unit")]
        public string RateUnit { get; set; }

        [Required(ErrorMessage = "Rate amount required.")]
        public double RateAmount { get; set; }

        [Required(ErrorMessage = "Class required.")]
        public int RateClass { get; set; }

        public bool Active { get; set; }

        [Required(ErrorMessage = "Please Select Client")]
        public int ClientID { get; set; }

        [Required(ErrorMessage = "Start date required.")]
        public System.DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Please select Department.")]
        public Nullable<int> DepartmentID { get; set; }

        [Required(ErrorMessage = "Required.")]
        [Display(Name="Client Billing Amount")]
        public double ClientBillAmount { get; set; }

        [Required(ErrorMessage = "Required.")]
        [Display(Name="Start Date")]
        public System.DateTime CBStartDate { get; set; }

        [Required(ErrorMessage = "Required.")]
        [Display(Name = "End Date")]
        public System.DateTime CBEndDate { get; set; }

        [Required(ErrorMessage = "Required.")]
        [Display(Name="Payment Amount")]
        public double PayFromAmount { get; set; }

        [Required(ErrorMessage = "Required.")]
        [Display(Name = "Start Date")]
        public System.DateTime PFStartDate { get; set; }

        [Required(ErrorMessage = "Required.")]
        [Display(Name = "End Date")]
        public System.DateTime PFEndDate { get; set; }

        [Display(Name = "Has Materials")]
        public bool HasMaterials { get; set; }

        [Display(Name = "Materials Required")]
        public bool MaterialsRequired { get; set; }

        [Display(Name = "Alt Code")]
        public string AltCode { get; set; }
    }
}