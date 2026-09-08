using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");

        builder.HasKey(e => e.Id);

        // No store default on any column of this table, and that is the rule of D34 rather than an
        // oversight: a boolean column is IsRequired() and nothing else, and the columns here are
        // written by the handler that caused them, always, so a default would only ever hide a
        // write that forgot to say something.
        builder.Property(e => e.OccurredAtUtc).IsRequired();
        builder.Property(e => e.ActorEmail).HasMaxLength(256).IsRequired();
        builder.Property(e => e.EntityType).HasMaxLength(40).IsRequired();
        builder.Property(e => e.EntityId).IsRequired();
        builder.Property(e => e.Action).IsRequired();
        builder.Property(e => e.Summary).HasMaxLength(400).IsRequired();

        // The only read is "the most recent ones", so the index is ordered the way that read is.
        builder.HasIndex(e => e.OccurredAtUtc).IsDescending();
    }
}
