using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<TaxRule> TaxRules => Set<TaxRule>();
        public DbSet<RuleAudit> RuleAudits => Set<RuleAudit>();
    }
}
