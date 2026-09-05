using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("ProductTranslations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Culture).HasMaxLength(10).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(120).IsRequired();
        builder.Property(t => t.Tagline).HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Highlights).IsRequired();

        builder.HasIndex(t => new { t.ProductId, t.Culture }).IsUnique();
    }
}
