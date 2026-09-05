using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class PricingTierConfiguration : IEntityTypeConfiguration<PricingTier>
{
    public void Configure(EntityTypeBuilder<PricingTier> builder)
    {
        builder.ToTable("PricingTiers", t =>
        {
            t.HasCheckConstraint("CK_PricingTiers_MinDays", "[MinDays] >= 1");
            t.HasCheckConstraint("CK_PricingTiers_MaxDays", "[MaxDays] IS NULL OR [MaxDays] >= [MinDays]");
            t.HasCheckConstraint("CK_PricingTiers_Amount", "[Amount] > 0");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.MinDays).IsRequired();
        builder.Property(t => t.Mode).IsRequired();
        builder.Property(t => t.Amount).HasPrecision(10, 2).IsRequired();

        builder.HasIndex(t => new { t.ProductId, t.MinDays });
    }
}
