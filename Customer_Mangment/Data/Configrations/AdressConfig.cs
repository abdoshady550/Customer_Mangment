using Customer_Mangment.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer_Mangment.Data.Configrations
{
    public class AdressConfig : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses", b => b.IsTemporal(
                     t =>
                     {
                         t.HasPeriodStart("ValidFrom");
                         t.HasPeriodEnd("ValidTo");
                         t.UseHistoryTable("AddressHistory");
                     }));

        }
    }

}
