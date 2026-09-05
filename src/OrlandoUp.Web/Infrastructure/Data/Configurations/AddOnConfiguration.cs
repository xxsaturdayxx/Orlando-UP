using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class AddOnConfiguration : IEntityTypeConfiguration<AddOn>
{
    public void Configure(EntityTypeBuilder<AddOn> builder)
    {
        builder.ToTable("AddOns");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code).HasMaxLength(40).IsRequired();
        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.PricingMode).IsRequired();
        builder.Property(a => a.Amount).HasPrecision(10, 2).IsRequired();
        builder.Property(a => a.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(a => a.SortOrder).IsRequired();

        builder.HasMany(a => a.Translations)
            .WithOne(t => t.AddOn!)
            .HasForeignKey(t => t.AddOnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
