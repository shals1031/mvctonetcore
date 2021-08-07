using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN17110
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecId { get; set; }
        public int QuotationId { get; set; }
        public string ActivityDesc { get; set; }
        public decimal ActQty { get; set; }
        public double? ActivityRate { get; set; }

        public virtual TRN17100 TRN17100 { get; set; }
    }
}
