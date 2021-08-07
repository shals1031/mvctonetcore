using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN09110
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WoActID { get; set; }
        public int InvoiceNum { get; set; }
        public decimal InvoicedAmount { get; set; }

        public virtual TRN09100 TRN09100 { get; set; }
        public virtual TRN23110 TRN23110 { get; set; }
    }
}
