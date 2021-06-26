using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public class CustomerEquipments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerEquipmentId { get; set; }
        public int CustomerId { get; set; }
        [StringLength(200)]
        public string MacDetail { get; set; }
        [StringLength(100)]
        public string SerialNo { get; set; }
        [StringLength(200)]
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public virtual Customer Customer { get; set; }
    }
}
