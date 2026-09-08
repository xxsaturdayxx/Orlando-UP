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

        builder.Property(p => p.WidthIn).HasPrecision(5, 1);
        builder.Property(p => p.LengthIn).HasPrecision(5, 1);
        builder.Property(p => p.SeatWidthIn).HasPrecision(5, 1);
        builder.Property(p => p.RangeMiles).HasPrecision(5, 1);

        builder.Property(p => p.TurnaroundDays).HasDefaultValue(0).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        // No store default on this one, on purpose, and the reason is a trap rather than a taste.
        // A store default on a non-nullable bool makes the provider unable to tell "the caller said
        // false" from "the caller said nothing": the false is dropped and the row is written with
        // the default. For this column that would insert a product as purchasable precisely when
        // the code asked for the opposite, which is the whole defect D32 exists to prevent. The
        // rows that already exist are filled by an explicit statement in the migration instead,
        // where a reviewer can read it.
        builder.Property(p => p.IsBookable).IsRequired();
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
