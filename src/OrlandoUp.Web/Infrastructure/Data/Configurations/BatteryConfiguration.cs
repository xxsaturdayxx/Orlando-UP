using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class BatteryConfiguration : IEntityTypeConfiguration<Battery>
{
    public void Configure(EntityTypeBuilder<Battery> builder)
    {
        builder.ToTable("Batteries");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.AssetTag).HasMaxLength(40).IsRequired();
        builder.HasIndex(b => b.AssetTag).IsUnique();

        builder.Property(b => b.Kind).IsRequired();
        builder.Property(b => b.RangeMiles).HasPrecision(5, 1);
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.SerialNumber).HasMaxLength(80);
        builder.Property(b => b.Notes).HasMaxLength(400);

        // A calendar date in Orlando, so a date column and not an instant (D16).
        builder.Property(b => b.PurchasedOn).HasColumnType("date");

        builder.Property(b => b.CreatedAtUtc).IsRequired();

        // Restrict, like units: a model that has batteries is not deleted out from under them.
        builder.HasOne(b => b.Product)
            .WithMany()
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
