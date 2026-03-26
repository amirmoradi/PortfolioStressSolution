using Microsoft.EntityFrameworkCore;
using PortfolioStress.Web.Domain.Entities;

namespace PortfolioStress.Web.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ScenarioRun> ScenarioRuns => Set<ScenarioRun>();
    public DbSet<ScenarioCountryInput> ScenarioCountryInputs => Set<ScenarioCountryInput>();
    public DbSet<ScenarioPortfolioResult> ScenarioPortfolioResults => Set<ScenarioPortfolioResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScenarioRun>(entity =>
        {
            entity.ToTable("ScenarioRuns");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CalculationProfile).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScenarioCollateralFormula).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RecoveryRateBase).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourcePortfoliosPath).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.SourceLoansPath).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.SourceRatingsPath).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.TotalOutstandingAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalCollateralValue).HasPrecision(18, 2);
            entity.Property(x => x.TotalScenarioCollateralValue).HasPrecision(18, 2);
            entity.Property(x => x.TotalExpectedLoss).HasPrecision(18, 2);
            entity.HasMany(x => x.CountryInputs).WithOne(x => x.ScenarioRun).HasForeignKey(x => x.ScenarioRunId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.PortfolioResults).WithOne(x => x.ScenarioRun).HasForeignKey(x => x.ScenarioRunId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScenarioCountryInput>(entity =>
        {
            entity.ToTable("ScenarioCountryInputs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ScenarioRunId, x.CountryCode }).IsUnique();
            entity.Property(x => x.CountryCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.PercentageChange).HasPrecision(9, 4);
        });

        modelBuilder.Entity<ScenarioPortfolioResult>(entity =>
        {
            entity.ToTable("ScenarioPortfolioResults");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ScenarioRunId, x.PortfolioId }).IsUnique();
            entity.Property(x => x.PortfolioName).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CountryCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.TotalOutstandingAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalCollateralValue).HasPrecision(18, 2);
            entity.Property(x => x.TotalScenarioCollateralValue).HasPrecision(18, 2);
            entity.Property(x => x.TotalExpectedLoss).HasPrecision(18, 2);
        });
    }
}
