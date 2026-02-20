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
                   .IsUnique();

            builder.HasMany(c => c.Addresses)
                   .WithOne(a => a.Customer)
                   .HasForeignKey(a => a.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.CustomerHistory)
                   .WithOne(h => h.Customer)
                   .HasForeignKey(h => h.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(c => !c.IsDeleted);

        }
    }
}
