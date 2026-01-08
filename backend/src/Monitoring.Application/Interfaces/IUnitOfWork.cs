using Monitoring.Domain.Entities;

namespace Monitoring.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<SystemMetric> SystemMetrics { get; }
    IRepository<KpiResult> KpiResults { get; }
    IRepository<Anomaly> Anomalies { get; }
    Task<int> SaveChangesAsync();
}
