using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public class Users
    {
        public Users()
        {
            TRN17100 = new HashSet<TRN17100>();
            TRN19100 = new HashSet<TRN19100>();
            TRN23100 = new HashSet<TRN23100>();
            UsersInRoles = new HashSet<UsersInRoles>();
        }

        public string ApplicationId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [StringLength(36)]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        [StringLength(50)]
        [Required]
        public string UserName { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }

        public virtual Applications Application { get; set; }
        public virtual Memberships Membership { get; set; }
        public virtual Profiles Profiles { get; set; }
        public virtual ICollection<UsersInRoles> UsersInRoles { get; set; }
        public virtual ICollection<TRN17100> TRN17100 { get; set; }
        public virtual ICollection<TRN19100> TRN19100 { get; set; }
        public virtual ICollection<TRN23100> TRN23100 { get; set; }
    }
}