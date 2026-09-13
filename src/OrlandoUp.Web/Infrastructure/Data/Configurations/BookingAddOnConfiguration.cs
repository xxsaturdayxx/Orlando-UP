using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class BookingAddOnConfiguration : IEntityTypeConfiguration<BookingAddOn>
{
    public void Configure(EntityTypeBuilder<BookingAddOn> builder)
    {
        builder.ToTable("BookingAddOns");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AddOnName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.PricingMode).IsRequired();
        builder.Property(a => a.Amount).HasPrecision(10, 2).IsRequired();
        builder.Property(a => a.Quantity).IsRequired();
        builder.Property(a => a.Total).HasPrecision(10, 2).IsRequired();

        // Cascade through the line, which cascades from the booking.
        builder.HasOne(a => a.BookingLine)
            .WithMany(l => l.AddOns)
            .HasForeignKey(a => a.BookingLineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: an extra is deactivated, never deleted.
        builder.HasOne(a => a.AddOn)
            .WithMany()
            .HasForeignKey(a => a.AddOnId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
