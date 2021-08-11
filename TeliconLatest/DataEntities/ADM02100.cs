using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeliconLatest.DataEntities
{
    public partial class ADM02100
    {
        public ADM02100()
        {
            ADM02200 = new HashSet<ADM02200>();
        }
        [Key]
        [Required]
        [Display(Name = "Code")]
        [StringLength(5)]
        public string BankId { get; set; }
        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string BankName { get; set; }

        public virtual ICollection<ADM02200> ADM02200 { get; set; }
    }

    public class ADM02100Extended : ADM02100
    {
        public ADM02100Extended() { }
        public ADM02100Extended(ADM02100 obj)
        {
            OldBankId = obj.BankId;
            BankId = obj.BankId;
            BankName = obj.BankName;
        }

        public ADM02100 GetBase()
        {
            return new ADM02100
            {
                BankId = BankId,
                BankName = BankName,
            };
        }

        public string OldBankId { get; set; }
    }
}