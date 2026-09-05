using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class DeliveryZoneTranslationConfiguration : IEntityTypeConfiguration<DeliveryZoneTranslation>
{
    public void Configure(EntityTypeBuilder<DeliveryZoneTranslation> builder)
    {
        builder.ToTable("DeliveryZoneTranslations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Culture).HasMaxLength(10).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(120).IsRequired();

        builder.HasIndex(t => new { t.ZoneId, t.Culture }).IsUnique();
    }
}
