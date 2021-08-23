using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM18100
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int remarkID { get; set; }
        [StringLength(100)]
        public string remarkText { get; set; }
    }
}
