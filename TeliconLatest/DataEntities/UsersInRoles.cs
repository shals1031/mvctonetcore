namespace TeliconLatest.DataEntities
{
    public class UsersInRoles
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public virtual Roles Roles { get; set; }
        public virtual Users Users { get; set; }
    }
}
