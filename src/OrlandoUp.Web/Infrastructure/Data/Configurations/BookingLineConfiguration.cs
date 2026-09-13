using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class BookingLineConfiguration : IEntityTypeConfiguration<BookingLine>
{
    public void Configure(EntityTypeBuilder<BookingLine> builder)
    {
        builder.ToTable("BookingLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Quantity).IsRequired();
        builder.Property(l => l.ExtraBatteryCount).IsRequired();

        builder.Property(l => l.TierMinDays).IsRequired();
        builder.Property(l => l.TierMode).IsRequired();
        builder.Property(l => l.TierAmount).HasPrecision(10, 2).IsRequired();

        builder.Property(l => l.UnitPrice).HasPrecision(10, 2).IsRequired();
        builder.Property(l => l.LineTotal).HasPrecision(10, 2).IsRequired();
        builder.Property(l => l.ExtraBatteryPerDay).HasPrecision(10, 2).IsRequired();
        builder.Property(l => l.ExtraBatteriesTotal).HasPrecision(10, 2).IsRequired();

        // Cascade: a line never outlives its booking, and there is nothing to keep about one whose
        // booking is gone.
        builder.HasOne(l => l.Booking)
            .WithMany(b => b.Lines)
            .HasForeignKey(l => l.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: the catalog row is hidden rather than deleted, so the link survives.
        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
