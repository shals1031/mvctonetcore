using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public class Memberships
    {
        [StringLength(36)]
        public string ApplicationId { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [StringLength(36)]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        [StringLength(250)]
        public string Password { get; set; }
        [StringLength(512)]
        public string Email { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        [StringLength(512)]
        public string Comment { get; set; }

        public virtual Applications Applications { get; set; }
        public virtual Users Users { get; set; }
    }
}