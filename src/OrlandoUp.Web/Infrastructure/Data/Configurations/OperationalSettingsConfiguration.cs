using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class OperationalSettingsConfiguration : IEntityTypeConfiguration<OperationalSettings>
{
    public void Configure(EntityTypeBuilder<OperationalSettings> builder)
    {
        builder.ToTable("OperationalSettings");

        // The key is written by whoever inserts the row, never generated: there is one row and its
        // key is a constant, so a table that could hand out a second key would be describing a list.
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.ChargerCount).IsRequired();
        builder.Property(s => s.SecondBatteryPerDay).HasPrecision(10, 2).IsRequired();
        builder.Property(s => s.LostChargerFee).HasPrecision(10, 2).IsRequired();

        // Required, and with no default declared here on purpose (D34): the migration writes 18
        // into the one row that already exists and then removes the constraint the tool leaves
        // behind, so the column's value is a fact about the operation and never something the
        // database supplies on the next insert.
        builder.Property(s => s.NextDayCutoffHour).IsRequired();
    }
}
