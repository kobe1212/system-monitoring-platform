using Monitoring.Application.DTOs;

namespace Monitoring.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(int hours = 24);
    Task<List<TimeSeriesDataDto>> GetMetricTrendAsync(string metricName, int hours = 24);
    Task<List<ServerMetricsDto>> GetServerHealthAsync();
}
