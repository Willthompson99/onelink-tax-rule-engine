using Microsoft.EntityFrameworkCore;
using onelink_tax_rule_engine.Api.Models;

namespace onelink_tax_rule_engine.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Claim> Claims         => Set<Claim>();
    public DbSet<ClaimType> ClaimTypes => Set<ClaimType>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // decimal precision warning-fix
        b.Entity<Claim>().Property(c => c.ClaimAmount)
                         .HasColumnType("decimal(10,2)");

        // seed claim-types
        b.Entity<ClaimType>().HasData(
            new ClaimType { ClaimTypeID = 1, TypeName = "Auto"   },
            new ClaimType { ClaimTypeID = 2, TypeName = "Home"   },
            new ClaimType { ClaimTypeID = 3, TypeName = "Health" },
            new ClaimType { ClaimTypeID = 4, TypeName = "Travel" },
            new ClaimType { ClaimTypeID = 5, TypeName = "Life"   }
        );
    }
}
