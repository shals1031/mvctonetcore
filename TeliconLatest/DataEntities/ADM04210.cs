using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM04210
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepActID { get; set; }
        public int DepartmentID { get; set; }
        public int ActivityID { get; set; }

        public virtual ADM01100 ADM01100 { get; set; }
        public virtual ADM04200 ADM04200 { get; set; }
    }
}
