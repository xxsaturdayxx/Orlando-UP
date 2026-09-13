using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class BookingEventConfiguration : IEntityTypeConfiguration<BookingEvent>
{
    public void Configure(EntityTypeBuilder<BookingEvent> builder)
    {
        builder.ToTable("BookingEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OccurredAtUtc).IsRequired();
        builder.Property(e => e.ActorEmail).HasMaxLength(256);
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Summary).HasMaxLength(500).IsRequired();

        builder.HasOne(e => e.Booking)
            .WithMany(b => b.Events)
            .HasForeignKey(e => e.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // The detail screen reads one booking's history in order, which is what this serves.
        builder.HasIndex(e => new { e.BookingId, e.OccurredAtUtc });
    }
}
