using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Slug).HasMaxLength(80).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Category).IsRequired();

        builder.Property(p => p.WidthIn).HasPrecision(5, 1).IsRequired();
        builder.Property(p => p.LengthIn).HasPrecision(5, 1).IsRequired();
        builder.Property(p => p.SeatWidthIn).HasPrecision(5, 1);
        builder.Property(p => p.RangeMiles).HasPrecision(5, 1);

        builder.Property(p => p.TurnaroundDays).HasDefaultValue(0).IsRequired();
        builder.Property(p => p.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(p => p.SortOrder).IsRequired();

        builder.Property(p => p.ImagePath).HasMaxLength(260);

        builder.Property(p => p.CreatedAtUtc).IsRequired();

        // The transport badge is a reading of the dimensions, not a column: a stored copy would
        // drift the day the dimensions are corrected. Same for the advertised daily price.
        builder.Ignore(p => p.FitsDisneyTransport);

        builder.HasMany(p => p.Translations)
            .WithOne(t => t.Product!)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PricingTiers)
            .WithOne(t => t.Product!)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Units)
            .WithOne(u => u.Product!)
            .HasForeignKey(u => u.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
