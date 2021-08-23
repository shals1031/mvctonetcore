using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public partial class ADM02300
    {
        //public ADM02300()
        //{
        //    TRN09100 = new HashSet<TRN09100>();
        //}
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BatchID { get; set; }
        public DateTime BatchDate { get; set; }
        //public virtual ICollection<TRN09100> TRN09100 { get; set; }
    }
}