using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM03200
    {
        public ADM03200()
        {
            TRN23100 = new HashSet<TRN23100>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustID { get; set; }
        [Required]
        [Display(Name = "Customer Name")]
        [StringLength(50)]
        public string CustName { get; set; }
        [StringLength(30)]
        public string Street { get; set; }
        [StringLength(30)]
        public string City { get; set; }
        [StringLength(30)]
        public string Parish { get; set; }
        [Display(Name = "Phone 1")]
        [StringLength(15)]
        public string Phone1 { get; set; }
        [Display(Name = "Phone 2")]
        [StringLength(15)]
        public string Phone2 { get; set; }
        [StringLength(15)]
        public string Fax { get; set; }
        [EmailAddress]
        [Required]
        [StringLength(50)]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Type")]
        public int CustClass { get; set; }
        [Display(Name = "Customer Code")]
        [Required]
        [Remote("ClientIDExists", "Client", HttpMethod = "Post", ErrorMessage = "Customer Code Already Exists", AdditionalFields = "initialID")]
        [StringLength(6, MinimumLength = 3)]
        public string ClientCode { get; set; }
        [StringLength(3)]
        public string Currency { get; set; }
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? StartDt { get; set; }
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? EndDt { get; set; }

        public virtual ICollection<TRN23100> TRN23100 { get; set; }
    }

    public class ADM03200Extended : ADM03200
    {
        public ADM03200Extended() { }
        public ADM03200Extended(ADM03200 obj)
        {
            CustID = obj.CustID;
            CustName = obj.CustName;
            Street = obj.Street;
            City = obj.City;
            Parish = obj.Parish;
            Phone1 = obj.Phone1;
            Phone2 = obj.Phone2;
            Fax = obj.Fax;
            Email = obj.Email;
            CustClass = obj.CustClass;
            ClientCode = obj.ClientCode;
            Currency = obj.Currency;
            StartDt = obj.StartDt;
            EndDt = obj.EndDt;
        }

        public ADM03200 GetBase()
        {
            return new ADM03200
            {
                CustID = CustID,
                CustName = CustName,
                Street = Street,
                City = City,
                Parish = Parish,
                Phone1 = Phone1,
                Phone2 = Phone2,
                Fax = Fax,
                Email = Email,
                CustClass = CustClass,
                ClientCode = ClientCode,
                Currency = Currency,
                StartDt = StartDt,
                EndDt = EndDt,
            };
        }

        [Display(Name = "Copy Activity From")]
        public int? CopyActClientId { get; set; }
    }
}