using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Number).HasMaxLength(16).IsRequired();
        builder.HasIndex(b => b.Number).IsUnique();

        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.Source).IsRequired();
        builder.Property(b => b.Culture).HasMaxLength(5).IsRequired();

        builder.Property(b => b.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.LastName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Email).HasMaxLength(256).IsRequired();
        builder.Property(b => b.Phone).HasMaxLength(40).IsRequired();

        builder.Property(b => b.Address).HasMaxLength(300);
        builder.Property(b => b.DeliveryNotes).HasMaxLength(500);

        // Calendar dates in Orlando, not instants (D16).
        builder.Property(b => b.StartDate).HasColumnType("date").IsRequired();
        builder.Property(b => b.EndDate).HasColumnType("date").IsRequired();

        builder.Property(b => b.DeliveryWindow).IsRequired();
        builder.Property(b => b.PickupWindow).IsRequired();
        builder.Property(b => b.Days).IsRequired();

        builder.Property(b => b.Subtotal).HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.ExtraBatteriesTotal).HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.AddOnsTotal).HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.DeliveryFee).HasPrecision(10, 2).IsRequired();

        // Four decimal places, mirroring the zone column this was copied from — a rate is not an
        // amount, and rounding it to cents would lose a quarter of a point of tax.
        builder.Property(b => b.TaxRate).HasPrecision(5, 4).IsRequired();

        builder.Property(b => b.Tax).HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.Total).HasPrecision(10, 2).IsRequired();

        // No store default, in either direction (D34): the flag's value is the C# type default and
        // the database is never the one that decides it.
        builder.Property(b => b.IsOverbooked).IsRequired();

        builder.Property(b => b.StaffNotes).HasMaxLength(1000);
        builder.Property(b => b.CreatedAtUtc).IsRequired();
        builder.Property(b => b.CreatedByEmail).HasMaxLength(256);
        builder.Property(b => b.CancelReason).HasMaxLength(500);

        // Restrict on both: a zone or a location that a booking points at is deactivated, never
        // deleted, and the booking must keep naming where it was delivered.
        builder.HasOne(b => b.DeliveryZone)
            .WithMany()
            .HasForeignKey(b => b.DeliveryZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.DeliveryLocation)
            .WithMany()
            .HasForeignKey(b => b.DeliveryLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // The availability loader filters on the overlap of a padded window with these two columns
        // and on the status, which is what these two indexes serve.
        builder.HasIndex(b => new { b.StartDate, b.EndDate });
        builder.HasIndex(b => b.Status);
    }
}
