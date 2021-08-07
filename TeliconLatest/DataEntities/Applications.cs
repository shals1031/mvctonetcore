using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public class Applications
    {
        public Applications()
        {
            Memberships = new HashSet<Memberships>();
            Roles = new HashSet<Roles>();
            Users = new HashSet<Users>();
        }
        [StringLength(235)]
        [Required]
        public string ApplicationName { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [StringLength(36)]
        public string ApplicationId { get; set; } = Guid.NewGuid().ToString();
        [StringLength(256)]
        public string Description { get; set; }

        public virtual ICollection<Memberships> Memberships { get; set; }
        public virtual ICollection<Roles> Roles { get; set; }
        public virtual ICollection<Users> Users { get; set; }
    }
}