using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data.Configurations;

public sealed class ProductAddOnConfiguration : IEntityTypeConfiguration<ProductAddOn>
{
    public void Configure(EntityTypeBuilder<ProductAddOn> builder)
    {
        builder.ToTable("ProductAddOns");

        builder.HasKey(pa => new { pa.ProductId, pa.AddOnId });

        builder.HasOne(pa => pa.Product!)
            .WithMany(p => p.AddOns)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.AddOn!)
            .WithMany(a => a.Products)
            .HasForeignKey(pa => pa.AddOnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
