using Microsoft.EntityFrameworkCore;
using Monitoring.Domain.Entities;

namespace Monitoring.Infrastructure.Data;

public class MonitoringDbContext : DbContext
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options) : base(options)
    {
    }

    public DbSet<SystemMetric> SystemMetrics { get; set; }
    public DbSet<KpiResult> KpiResults { get; set; }
    public DbSet<Anomaly> Anomalies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SystemMetric>(entity =>
        {
            entity.ToTable("SystemMetrics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Timestamp).IsRequired();
            
            entity.HasIndex(e => e.MetricName);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.MetricName, e.Timestamp });
        });

        modelBuilder.Entity<KpiResult>(entity =>
        {
            entity.ToTable("KpiResults");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KpiName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CalculatedValue).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CalculatedAt).IsRequired();
            entity.Property(e => e.PeriodStart).IsRequired();
            entity.Property(e => e.PeriodEnd).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.KpiName);
            entity.HasIndex(e => e.CalculatedAt);
        });

        modelBuilder.Entity<Anomaly>(entity =>
        {
            entity.ToTable("Anomalies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DetectedValue).IsRequired();
            entity.Property(e => e.ExpectedValue).IsRequired();
            entity.Property(e => e.Deviation).IsRequired();
            entity.Property(e => e.Severity).IsRequired();
            entity.Property(e => e.DetectedAt).IsRequired();
            entity.Property(e => e.IsResolved).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.MetricName);
            entity.HasIndex(e => e.DetectedAt);
            entity.HasIndex(e => e.IsResolved);
        });
    }
}
