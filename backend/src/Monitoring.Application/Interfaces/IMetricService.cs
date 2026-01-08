using Monitoring.Application.DTOs;

namespace Monitoring.Application.Interfaces;

public interface IMetricService
{
    Task<SystemMetricDto> CreateMetricAsync(CreateMetricDto createMetricDto);
    Task<IEnumerable<SystemMetricDto>> GetMetricsAsync(MetricQueryDto query);
    Task<SystemMetricDto?> GetMetricByIdAsync(int id);
    Task<IEnumerable<SystemMetricDto>> GetRecentMetricsAsync(int count = 100);
}
