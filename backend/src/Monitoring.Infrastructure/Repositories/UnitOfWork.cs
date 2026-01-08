using Monitoring.Application.Interfaces;
using Monitoring.Domain.Entities;
using Monitoring.Infrastructure.Data;

namespace Monitoring.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MonitoringDbContext _context;
    private IRepository<SystemMetric>? _systemMetrics;
    private IRepository<KpiResult>? _kpiResults;
    private IRepository<Anomaly>? _anomalies;

    public UnitOfWork(MonitoringDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<SystemMetric> SystemMetrics
    {
        get
        {
            _systemMetrics ??= new Repository<SystemMetric>(_context);
            return _systemMetrics;
        }
    }

    public IRepository<KpiResult> KpiResults
    {
        get
        {
            _kpiResults ??= new Repository<KpiResult>(_context);
            return _kpiResults;
        }
    }

    public IRepository<Anomaly> Anomalies
    {
        get
        {
            _anomalies ??= new Repository<Anomaly>(_context);
            return _anomalies;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
