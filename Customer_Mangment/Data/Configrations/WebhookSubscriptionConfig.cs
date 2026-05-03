using Customer_Mangment.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer_Mangment.Data.Configurations;

public sealed class WebhookSubscriptionConfig
    : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("WebhookSubscriptions");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(w => w.Events)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(w => w.Secret)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(w => w.IsActive);
    }
}