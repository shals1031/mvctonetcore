using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN13110
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MergedSubOrderId { get; set; }
        public int MergedOrderId { get; set; }
        public int WorkOrderId { get; set; }

        public virtual TRN13120 TRN13120 { get; set; }
        public virtual TRN23100 TRN23100 { get; set; }
    }
}
