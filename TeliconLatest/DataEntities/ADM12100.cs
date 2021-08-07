using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM12100
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int locationID { get; set; }
        [StringLength(35)]
        public string locationName { get; set; }
    }
}
