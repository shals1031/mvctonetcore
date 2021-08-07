using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM01120
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int splitID { get; set; }
        public int activityID { get; set; }
        public double OVSenior1 { get; set; }
        public double OVSenior2 { get; set; }
        public double OVJunior2 { get; set; }
        public double CVSenior { get; set; }
        public double CVJunior { get; set; }
    }
}