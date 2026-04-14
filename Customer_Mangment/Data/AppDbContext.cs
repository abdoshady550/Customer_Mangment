using Customer_Mangment.Model.Entities;
using Customer_Mangment.MultiTenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Customer_Mangment.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, TenantContext? tenantContext = null)
        : IdentityDbContext<User>(options)
    {
        private readonly TenantContext? _tenantContext = tenantContext;

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public string? getTenantId() => _tenantContext?.TenantId;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<Customer>()
                        .HasQueryFilter(c =>
                            !c.IsDeleted &&
                            (_tenantContext == null || !_tenantContext.IsResolved || c.TenantId == _tenantContext.TenantId));

            modelBuilder.Entity<Address>()
                        .HasQueryFilter(a => !a.Customer.IsDeleted &&
                        (_tenantContext == null || !_tenantContext.IsResolved || a.Customer.TenantId == _tenantContext.TenantId));

            modelBuilder.Entity<User>()
                    .Ignore(u => u.ConcurrencyStamp);
        }

        public override int SaveChanges()
        {
            StampTenantId();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            StampTenantId();
            return base.SaveChangesAsync(ct);
        }

        private void StampTenantId()
        {
            if (_tenantContext is not { IsResolved: true })
                return;

            foreach (var entry in ChangeTracker.Entries<Customer>()
                         .Where(e => e.State == EntityState.Added))
            {
                if (string.IsNullOrEmpty(entry.Entity.TenantId))
                    entry.Entity.TenantId = _tenantContext.TenantId;
            }
        }
    }
}
