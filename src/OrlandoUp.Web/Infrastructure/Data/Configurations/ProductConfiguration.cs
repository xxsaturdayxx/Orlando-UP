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

        // No store default here, and none on IsActive either since leva 04 (D34, D35). The
        // mechanism is worth stating correctly, because the note this replaces stated it backwards
        // and the correct version is what makes the rule readable: EF decides whether to send a
        // column by comparing the property against its SENTINEL. Declaring the default by VALUE
        // moves the sentinel to that value, so an explicit false still differs from it and is
        // sent. Declaring it in SQL leaves the sentinel at the language default, so an explicit
        // false looks like silence, the column is omitted, and the database writes the default
        // over it — on a non-nullable bool that is how a product gets published when the code
        // asked for the opposite. Measured on EF Core 10.0.11 and recorded in
        // Docs/relatorio-leva-04-etapa-1.md. The rule that closes the door on the biting form is
        // simply: a boolean column is IsRequired() and nothing else.
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
