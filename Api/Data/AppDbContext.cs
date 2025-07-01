using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimType> ClaimTypes { get; set; }
        public DbSet<TaxRule> TaxRules { get; set; }
        public DbSet<RuleAudit> RuleAudits { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }
}
