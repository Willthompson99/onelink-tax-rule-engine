using Microsoft.EntityFrameworkCore;
using onelink_tax_rule_engine.Api.Models;

namespace onelink_tax_rule_engine.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Claim> Claims         => Set<Claim>();
        public DbSet<ClaimType> ClaimTypes => Set<ClaimType>();
    }
}
