using LSC.AZ204.WebAPI.Core;
using Microsoft.EntityFrameworkCore;

namespace LSC.AZ204.WebAPI.Data
{
    public class AZ204DemoDbContext: DbContext
    {
        public AZ204DemoDbContext(DbContextOptions<AZ204DemoDbContext> options)
           : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
         
        }

        public DbSet<CustomerContactUploads> CustomerContactUploads { get; set; }
    }
}
