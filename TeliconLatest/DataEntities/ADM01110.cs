using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM01110
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActMatID { get; set; }
        public int ActivID { get; set; }
        public int MaterID { get; set; }
        public int ActMatQty { get; set; }

        public virtual ADM01100 ADM01100 { get; set; }
        public virtual ADM13100 ADM13100 { get; set; }
    }
}