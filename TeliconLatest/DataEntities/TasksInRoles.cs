using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeliconLatest.DataEntities
{
    public class TasksInRoles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleTaskId { get; set; }
        public string RoleId { get; set; }
        public int TaskId { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
    }
}