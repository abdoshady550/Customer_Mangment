using Customer_Mangment.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer_Mangment.Data.Configrations
{
    public class CustomerConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(c => c.Mobile)
                   .HasMaxLength(15)
                   .IsRequired();

            builder.HasIndex(c => c.Mobile)
                   .IsUnique()
                   .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(c => c.TenantId);

            builder.HasMany(c => c.Addresses)
                   .WithOne(a => a.Customer)
                   .HasForeignKey(a => a.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Customers", b => b.IsTemporal(
                  t =>
                  {
                      t.HasPeriodStart("ValidFrom");
                      t.HasPeriodEnd("ValidTo");
                      t.UseHistoryTable("CustomerHistory");
                  }));

            builder.HasQueryFilter(c => !c.IsDeleted);

        }
    }

}
