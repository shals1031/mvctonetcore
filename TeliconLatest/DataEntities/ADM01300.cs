using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM01300
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int accountID { get; set; }
        [StringLength(70)]
        public string accountName { get; set; }
        [StringLength(70)]
        public string address1 { get; set; }
        [StringLength(70)]
        public string address2 { get; set; }
        [StringLength(70)]
        public string address3 { get; set; }
        [StringLength(2)]
        public string accType { get; set; }
        [StringLength(16)]
        public string accountNo { get; set; }
    }
}