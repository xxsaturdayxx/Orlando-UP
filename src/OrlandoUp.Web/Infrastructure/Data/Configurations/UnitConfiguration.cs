using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.AssetTag).HasMaxLength(40).IsRequired();
        builder.HasIndex(u => u.AssetTag).IsUnique();

        builder.Property(u => u.SerialNumber).HasMaxLength(80);
        builder.Property(u => u.Status).IsRequired();

        // A calendar date in Orlando, so a date column and not an instant (D16).
        builder.Property(u => u.PurchasedOn).HasColumnType("date");

        builder.Property(u => u.CreatedAtUtc).IsRequired();
    }
}
