using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN13120
    {
        public TRN13120()
        {
            TRN13110 = new HashSet<TRN13110>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MergedOrderId { get; set; }
        public int WorkOrderId { get; set; }
        public string MergedRefNum { get; set; }
        public string MergerdTitle { get; set; }
        public DateTime? MergerdDate { get; set; }
        public virtual TRN23100 TRN23100 { get; set; }
        public virtual ICollection<TRN13110> TRN13110 { get; set; }
    }
}