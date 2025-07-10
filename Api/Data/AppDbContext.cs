using Microsoft.EntityFrameworkCore;
using OklahomaTaxEngine.Models;

namespace OklahomaTaxEngine.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaxPayer> TaxPayers { get; set; }
        public DbSet<TaxRule> TaxRules { get; set; }
        public DbSet<TaxTransaction> TaxTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TaxPayer configuration
            modelBuilder.Entity<TaxPayer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TaxId).IsUnique();
                entity.Property(e => e.TaxId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(2);
                entity.Property(e => e.ZipCode).HasMaxLength(10);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // TaxRule configuration
            modelBuilder.Entity<TaxRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TaxType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RuleName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MinAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Rate).HasColumnType("decimal(5,4)").IsRequired();
                entity.Property(e => e.FlatAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.EffectiveFrom).IsRequired();
                entity.Property(e => e.Priority).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                
                // Add index for performance
                entity.HasIndex(e => new { e.TaxType, e.IsActive, e.EffectiveFrom });
            });

            // TaxTransaction configuration
            modelBuilder.Entity<TaxTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TaxType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TaxableAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.TransactionDate).IsRequired();
                entity.Property(e => e.TaxPeriod).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.Notes).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Foreign key relationship
                entity.HasOne(e => e.TaxPayer)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(e => e.TaxPayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add index for performance
                entity.HasIndex(e => new { e.TaxPayerId, e.TransactionDate });
                entity.HasIndex(e => e.Status);
            });

            // Add sample data for demonstration
            modelBuilder.Entity<TaxPayer>().HasData(
                new TaxPayer
                {
                    Id = 1,
                    TaxId = "OK987654321",
                    Name = "Demo Individual Taxpayer",
                    Type = "Individual",
                    Email = "demo@example.com",
                    Phone = "405-555-0123",
                    Address = "456 Demo Street",
                    City = "Stillwater",
                    State = "OK",
                    ZipCode = "74074",
                    RegistrationDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;
                entity.UpdatedAt = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }

    // Base entity class for common properties
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}