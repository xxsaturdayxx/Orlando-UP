using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>
/// The catalog tables plus the ASP.NET Core Identity tables under their default names. Identity is
/// for staff only (Docs/decisions.md D8/01 of the leva 01 spec); customers never get an account
/// in this phase.
/// </summary>
/// <remarks>
/// The schema is never brought into existence at start-up; that is what migrations are for, and
/// control C09 asserts that the start-up path stays free of it (D12).
/// </remarks>
public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<PricingTier> PricingTiers => Set<PricingTier>();

    public DbSet<AddOn> AddOns => Set<AddOn>();

    public DbSet<AddOnTranslation> AddOnTranslations => Set<AddOnTranslation>();

    public DbSet<ProductAddOn> ProductAddOns => Set<ProductAddOn>();

    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();

    public DbSet<DeliveryZoneTranslation> DeliveryZoneTranslations => Set<DeliveryZoneTranslation>();

    public DbSet<DeliveryLocation> DeliveryLocations => Set<DeliveryLocation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
