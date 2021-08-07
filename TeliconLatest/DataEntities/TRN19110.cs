using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN19110
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecId { get; set; }
        public int SInvoiceId { get; set; }
        public int ActivityId { get; set; }
        public decimal ActQty { get; set; }
        public double? ActivityRate { get; set; }

        public virtual ADM01100 ADM01100 { get; set; }
        public virtual TRN19100 TRN19100 { get; set; }
    }
}