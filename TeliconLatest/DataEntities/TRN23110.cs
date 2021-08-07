using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class TRN23110
    {
        public TRN23110()
        {
            TRN09110 = new HashSet<TRN09110>();
            TRN23120 = new HashSet<TRN23120>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecID { get; set; }
        public int WorkOID { get; set; }
        public int ActivityID { get; set; }
        public decimal ActQty { get; set; }
        public decimal OActQty { get; set; }
        [StringLength(100)]
        public string Location { get; set; }
        public DateTime ActDate { get; set; }
        public bool InvFlag { get; set; }
        public bool AddMaterial { get; set; }
        public DateTime? ActPaid { get; set; }
        [StringLength(400)]
        public string AdtnlDetails { get; set; }

        public virtual ADM01100 ADM01100 { get; set; }
        public virtual TRN23100 TRN23100 { get; set; }
        public virtual ICollection<TRN09110> TRN09110 { get; set; }
        public virtual ICollection<TRN23120> TRN23120 { get; set; }
    }
}