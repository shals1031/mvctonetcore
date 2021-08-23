using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN09101
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int newnum { get; set; }
        [StringLength(150)]
        public string InvoiceTitle { get; set; }
        public DateTime? GeneratedDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public int? BatchId { get; set; }
        [StringLength(2)]
        public string Status { get; set; }
        public int? NewInvNo { get; set; }
    }
}