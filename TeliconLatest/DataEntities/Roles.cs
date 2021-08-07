using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public class Roles
    {
        public Roles()
        {
            UsersInRoles = new HashSet<UsersInRoles>();
        }
        [StringLength(36)]
        public string ApplicationId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [StringLength(36)]
        public string RoleId { get; set; } = Guid.NewGuid().ToString();
        [StringLength(256)]
        [Required]
        public string RoleName { get; set; }
        [StringLength(256)]
        public string Description { get; set; }

        public virtual Applications Applications { get; set; }
        public virtual ICollection<UsersInRoles> UsersInRoles { get; set; }
    }
}