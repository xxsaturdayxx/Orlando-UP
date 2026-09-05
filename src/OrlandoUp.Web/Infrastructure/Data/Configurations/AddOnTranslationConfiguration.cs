using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class AddOnTranslationConfiguration : IEntityTypeConfiguration<AddOnTranslation>
{
    public void Configure(EntityTypeBuilder<AddOnTranslation> builder)
    {
        builder.ToTable("AddOnTranslations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Culture).HasMaxLength(10).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(120).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(400);

        builder.HasIndex(t => new { t.AddOnId, t.Culture }).IsUnique();
    }
}
