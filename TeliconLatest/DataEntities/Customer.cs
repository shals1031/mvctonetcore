using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }
        [StringLength(200)]
        [Required(ErrorMessage = "Customer Name is required")]
        public string Name { get; set; }
        [StringLength(100)]
        [Required(ErrorMessage = "Account No is required")]
        public string AccountNo { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime JobDate { get; set; }
        [StringLength(200)]
        [Required(ErrorMessage = "Tech Name is required")]
        public string TechName { get; set; }
        [StringLength(100)]
        [Required(ErrorMessage = "Tech Id is required")]
        public string TechId { get; set; }
        [StringLength(100)]
        [Required(ErrorMessage = "Service Order No is required")]
        public string ServiceOrdNo { get; set; }
        [StringLength(500)]
        [Required(ErrorMessage = "Customer Address is required")]
        public string CustAddress { get; set; }
        public bool IsWTNew { get; set; }
        public bool IsWTExisting { get; set; }
        public bool IsWTRewireCable { get; set; }
        public bool IsWTAddOutlet { get; set; }
        public bool IsWTServiceRepOrd { get; set; }
        public bool IsWTNotDone { get; set; }
        public bool IsWTServiceCall { get; set; }
        public bool IsSIWatch { get; set; }
        public bool IsSITalk { get; set; }
        public bool IsSIClick { get; set; }
        [StringLength(500)]
        public string CustComment { get; set; }
        [Required(ErrorMessage = "Time In is required")]
        public TimeSpan TimeIn { get; set; }
        [Required(ErrorMessage = "Time Out is required")]
        public TimeSpan TimeOut { get; set; }
        [Required(ErrorMessage = "Customer Signature is required")]
        [StringLength(500)]
        public string CustSignature { get; set; }
        public DateTime CustDate { get; set; }
        [Required(ErrorMessage = "Technician Signature is required")]
        [StringLength(500)]
        public string TechSignature { get; set; }
        public DateTime TechDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public virtual Collection<CustomerEquipments> CustomerEquipments { get; set; }
    }
}