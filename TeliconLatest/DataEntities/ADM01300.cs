using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM01300
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int accountID { get; set; }
        [StringLength(35)]
        public string accountName { get; set; }
        [StringLength(35)]
        public string address1 { get; set; }
        [StringLength(35)]
        public string address2 { get; set; }
        [StringLength(35)]
        public string address3 { get; set; }
        [StringLength(1)]
        public string accType { get; set; }
        [StringLength(8)]
        public string accountNo { get; set; }
    }
}