using Microsoft.EntityFrameworkCore;
using Api.Models;
using System.Text.Json;

namespace Api.Data;

public class TaxEngineDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public TaxEngineDbContext(DbContextOptions<TaxEngineDbContext> options, 
        IHttpContextAccessor httpContextAccessor) : base(options) 
    { 
        _httpContextAccessor = httpContextAccessor;
    }

    // Core entities
    public DbSet<TaxPayer> TaxPayers => Set<TaxPayer>();
    public DbSet<TaxType> TaxTypes => Set<TaxType>();
    public DbSet<TaxTransaction> TaxTransactions => Set<TaxTransaction>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<TaxAccount> TaxAccounts => Set<TaxAccount>();
    
    // Supporting entities
    public DbSet<TransactionLineItem> TransactionLineItems => Set<TransactionLineItem>();
    public DbSet<TaxRateSchedule> TaxRateSchedules => Set<TaxRateSchedule>();
    public DbSet<RuleApplication> RuleApplications => Set<RuleApplication>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    // Legacy entities (for migration)
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimType> ClaimTypes => Set<ClaimType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TaxPayer configuration
        modelBuilder.Entity<TaxPayer>(entity =>
        {
            entity.HasKey(e => e.TaxPayerId);
            entity.HasIndex(e => e.TaxPayerIdentifier).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.ZipCode).HasMaxLength(10);
        });

        // TaxType configuration
        modelBuilder.Entity<TaxType>(entity =>
        {
            entity.HasKey(e => e.TaxTypeId);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DefaultRate).HasColumnType("decimal(5,4)");
        });

        // TaxTransaction configuration
        modelBuilder.Entity<TaxTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.Property(e => e.GrossAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Deductions).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxableAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxRate).HasColumnType("decimal(5,4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PenaltyAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.InterestAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalDue).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.TaxPayer)
                .WithMany(p => p.Transactions)
                .HasForeignKey(e => e.TaxPayerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.TaxType)
                .WithMany(t => t.Transactions)
                .HasForeignKey(e => e.TaxTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TaxRule configuration
        modelBuilder.Entity<TaxRule>(entity =>
        {
            entity.HasKey(e => e.RuleId);
            entity.Property(e => e.RuleName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CriteriaJson).HasColumnType("nvarchar(max)");
        });

        // Seed data
        SeedTaxTypes(modelBuilder);
        SeedTaxRules(modelBuilder);
        
        // Keep legacy claim types for backward compatibility
        modelBuilder.Entity<Claim>()
            .Property(c => c.ClaimAmount)
            .HasColumnType("decimal(10,2)");
            
        modelBuilder.Entity<ClaimType>().HasData(
            new ClaimType { ClaimTypeID = 1, TypeName = "Auto" },
            new ClaimType { ClaimTypeID = 2, TypeName = "Home" },
            new ClaimType { ClaimTypeID = 3, TypeName = "Health" },
            new ClaimType { ClaimTypeID = 4, TypeName = "Travel" },
            new ClaimType { ClaimTypeID = 5, TypeName = "Life" }
        );
    }

    private static void SeedTaxTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaxType>().HasData(
            new TaxType 
            { 
                TaxTypeId = 1, 
                Code = "SALES", 
                Name = "Sales and Use Tax",
                Description = "Tax on retail sales of tangible personal property",
                Category = "State",
                DefaultRate = 0.045m, // 4.5% OK state rate
                FilingFrequency = "Monthly",
                FilingDeadlineDays = 20
            },
            new TaxType 
            { 
                TaxTypeId = 2, 
                Code = "INCOME", 
                Name = "Individual Income Tax",
                Description = "Tax on individual income",
                Category = "State",
                DefaultRate = 0.05m, // 5% top rate
                FilingFrequency = "Annual",
                FilingDeadlineDays = 105 // April 15
            },
            new TaxType 
            { 
                TaxTypeId = 3, 
                Code = "WITHHOLDING", 
                Name = "Employer Withholding Tax",
                Description = "Employer withholding of employee income tax",
                Category = "State",
                DefaultRate = 0.0m, // Variable based on employee
                FilingFrequency = "Monthly",
                FilingDeadlineDays = 20
            },
            new TaxType 
            { 
                TaxTypeId = 4, 
                Code = "FRANCHISE", 
                Name = "Franchise Tax",
                Description = "Tax on corporations doing business in Oklahoma",
                Category = "State",
                DefaultRate = 0.00125m, // $1.25 per $1,000
                FilingFrequency = "Annual",
                FilingDeadlineDays = 30
            }
        );
    }

    private static void SeedTaxRules(ModelBuilder modelBuilder)
    {
        var salesTaxExemptionRule = new
        {
            RuleId = 1,
            RuleName = "Grocery Sales Tax Exemption",
            RuleType = "Exemption",
            TaxTypeId = 1,
            CriteriaJson = JsonSerializer.Serialize(new
            {
                Conditions = new[]
                {
                    new { Field = "ItemCategory", Operator = "Equals", Value = "Grocery" }
                },
                Action = "ExemptFromTax"
            }),
            Priority = 100,
            EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };

        var latePenaltyRule = new
        {
            RuleId = 2,
            RuleName = "Late Filing Penalty",
            RuleType = "Penalty",
            TaxTypeId = 1,
            CriteriaJson = JsonSerializer.Serialize(new
            {
                Conditions = new[]
                {
                    new { Field = "DaysLate", Operator = "GreaterThan", Value = "0" }
                },
                Action = "ApplyPenalty",
                PenaltyRate = 0.05m
            }),
            Priority = 50,
            EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };

        // Note: We're creating anonymous types here because EF Core seed data 
        // doesn't work well with navigation properties
        modelBuilder.Entity<TaxRule>().HasData(salesTaxExemptionRule, latePenaltyRule);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            await CreateAuditLog(entry);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateAuditLog(EntityEntry entry)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        
        var auditLog = new AuditLog
        {
            EntityType = entry.Entity.GetType().Name,
            EntityId = GetPrimaryKeyValue(entry),
            Action = entry.State.ToString(),
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        if (entry.State == EntityState.Modified)
        {
            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (property.IsModified)
                {
                    oldValues[property.Metadata.Name] = property.OriginalValue ?? "null";
                    newValues[property.Metadata.Name] = property.CurrentValue ?? "null";
                }
            }

            auditLog.OldValues = JsonSerializer.Serialize(oldValues);
            auditLog.NewValues = JsonSerializer.Serialize(newValues);
        }
        else if (entry.State == EntityState.Added)
        {
            var values = entry.Properties
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue ?? "null");
            auditLog.NewValues = JsonSerializer.Serialize(values);
        }
        else if (entry.State == EntityState.Deleted)
        {
            var values = entry.Properties
                .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue ?? "null");
            auditLog.OldValues = JsonSerializer.Serialize(values);
        }

        await AuditLogs.AddAsync(auditLog);
    }

    private int GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyName = entry.Metadata.FindPrimaryKey()?.Properties
            .Select(x => x.Name).FirstOrDefault();

        if (keyName != null)
        {
            var keyValue = entry.Property(keyName).CurrentValue;
            if (keyValue != null && int.TryParse(keyValue.ToString(), out int id))
            {
                return id;
            }
        }

        return 0;
    }
}