using Microsoft.EntityFrameworkCore;

namespace TeliconLatest.DataEntities
{
    public class TeliconDbContext : DbContext
    {
        public TeliconDbContext(DbContextOptions<TeliconDbContext> options) : base(options)
        {
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerEquipments> CustomerEquipments { get; set; }
    }
}
