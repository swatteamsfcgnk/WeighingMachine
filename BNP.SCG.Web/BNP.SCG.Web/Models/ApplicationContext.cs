

using Microsoft.EntityFrameworkCore;

namespace BNP.SCG.Web.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options)
: base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
