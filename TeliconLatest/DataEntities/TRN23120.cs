using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN23120
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WoMatRecID { get; set; }
        public int WoMatID { get; set; }
        public int WoMatQty { get; set; }
        public int WoActID { get; set; }

        public virtual TRN23110 TRN23110 { get; set; }
    }
}