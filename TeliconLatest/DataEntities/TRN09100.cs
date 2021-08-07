using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN09100
    {
        public TRN09100()
        {
            TRN09110 = new HashSet<TRN09110>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceNum { get; set; }
        [StringLength(150)]
        public string InvoiceTitle { get; set; }
        public DateTime? GeneratedDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public int? BatchId { get; set; }
        [StringLength(1)]
        public string Status { get; set; }
        public int? NewInvNo { get; set; }
        public int? batchTMP { get; set; }
        [StringLength(1)]
        public string statusTMP { get; set; }
        public int InvNewNum { get; set; }
        public bool IsNewFormat { get; set; }
        public virtual ADM02300 ADM02300 { get; set; }
        public virtual ICollection<TRN09110> TRN09110 { get; set; }
    }
}