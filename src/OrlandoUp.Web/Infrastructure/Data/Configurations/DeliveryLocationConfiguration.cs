using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class DeliveryLocationConfiguration : IEntityTypeConfiguration<DeliveryLocation>
{
    public void Configure(EntityTypeBuilder<DeliveryLocation> builder)
    {
        builder.ToTable("DeliveryLocations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name).HasMaxLength(160).IsRequired();
        builder.Property(l => l.Address).HasMaxLength(300);
        builder.Property(l => l.Notes).HasMaxLength(400);
        builder.Property(l => l.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(l => l.SortOrder).IsRequired();

        builder.HasIndex(l => new { l.ZoneId, l.Name }).IsUnique();
    }
}
