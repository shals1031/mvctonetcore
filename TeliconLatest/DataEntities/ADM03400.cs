using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM03400
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecID { get; set; }
        public int WorkOrderId { get; set; }
        public int ContractorID { get; set; }
        public bool CrewLead { get; set; }
        public double ContractorRate { get; set; }
        public bool? IsHide { get; set; }

        public virtual ADM03300 ADM03300 { get; set; }
        public virtual TRN23100 TRN23100 { get; set; }
    }
}