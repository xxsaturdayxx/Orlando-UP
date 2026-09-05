using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class DeliveryZoneConfiguration : IEntityTypeConfiguration<DeliveryZone>
{
    public void Configure(EntityTypeBuilder<DeliveryZone> builder)
    {
        builder.ToTable("DeliveryZones");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Code).HasMaxLength(40).IsRequired();
        builder.HasIndex(z => z.Code).IsUnique();

        builder.Property(z => z.Kind).IsRequired();
        builder.Property(z => z.DeliveryFee).HasPrecision(10, 2).IsRequired();
        builder.Property(z => z.HandoverMode).IsRequired();
        builder.Property(z => z.SalesTaxRate).HasPrecision(6, 4).HasDefaultValue(0m).IsRequired();
        builder.Property(z => z.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(z => z.SortOrder).IsRequired();

        builder.HasMany(z => z.Translations)
            .WithOne(t => t.Zone!)
            .HasForeignKey(t => t.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(z => z.Locations)
            .WithOne(l => l.Zone!)
            .HasForeignKey(l => l.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
