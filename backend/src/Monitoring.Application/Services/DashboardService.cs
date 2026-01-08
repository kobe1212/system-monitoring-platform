using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;
using Monitoring.Domain.Entities;

namespace Monitoring.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(int hours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        var allMetrics = await _unitOfWork.SystemMetrics.FindAsync(m => m.Timestamp >= cutoffTime);
        var metrics = allMetrics.ToList();

        var analytics = new DashboardAnalyticsDto
        {
            Summary = await GetSummaryAsync(metrics),
            ResponseTimeTrend = GetResponseTimeTrend(metrics, hours),
            ThroughputTrend = GetThroughputTrend(metrics, hours),
            ServerMetrics = await GetServerHealthAsync(),
            MetricDistribution = GetMetricDistribution(metrics)
        };

        return analytics;
    }

    public async Task<List<TimeSeriesDataDto>> GetMetricTrendAsync(string metricName, int hours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        var allMetrics = await _unitOfWork.SystemMetrics.FindAsync(m => 
            m.MetricName == metricName && m.Timestamp >= cutoffTime);
        
        var metrics = allMetrics.OrderBy(m => m.Timestamp).ToList();

        var hourlyData = metrics
            .GroupBy(m => new DateTime(m.Timestamp.Year, m.Timestamp.Month, m.Timestamp.Day, m.Timestamp.Hour, 0, 0))
            .Select(g => new TimeSeriesDataDto
            {
                Timestamp = g.Key,
                Value = Math.Round(g.Average(m => m.Value), 2)
            })
            .OrderBy(d => d.Timestamp)
            .ToList();

        return hourlyData;
    }

    public async Task<List<ServerMetricsDto>> GetServerHealthAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        var allMetrics = await _unitOfWork.SystemMetrics.FindAsync(m => m.Timestamp >= cutoffTime);
        var recentMetrics = allMetrics.ToList();

        var serverGroups = recentMetrics.GroupBy(m => m.Source);

        var serverHealth = serverGroups.Select(g =>
        {
            var cpuMetrics = g.Where(m => m.MetricName == "CPUUsage");
            var memoryMetrics = g.Where(m => m.MetricName == "MemoryUsage");
            var responseMetrics = g.Where(m => m.MetricName == "ResponseTime");
            var requestMetrics = g.Where(m => m.MetricName == "RequestCount");

            var avgCpu = cpuMetrics.Any() ? cpuMetrics.Average(m => m.Value) : 0;
            var avgMemory = memoryMetrics.Any() ? memoryMetrics.Average(m => m.Value) : 0;
            var avgResponse = responseMetrics.Any() ? responseMetrics.Average(m => m.Value) : 0;
            var totalRequests = requestMetrics.Any() ? (int)requestMetrics.Sum(m => m.Value) : 0;

            var status = avgCpu > 80 || avgMemory > 85 || avgResponse > 300 ? "Critical" :
                        avgCpu > 70 || avgMemory > 75 || avgResponse > 200 ? "Warning" : "Healthy";

            return new ServerMetricsDto
            {
                ServerName = g.Key,
                CPUUsage = Math.Round(avgCpu, 2),
                MemoryUsage = Math.Round(avgMemory, 2),
                ResponseTime = Math.Round(avgResponse, 2),
                RequestCount = totalRequests,
                Status = status
            };
        }).ToList();

        return serverHealth;
    }

    private async Task<DashboardSummaryDto> GetSummaryAsync(List<SystemMetric> metrics)
    {
        var servers = metrics.Select(m => m.Source).Distinct().Count();
        var allAnomalies = await _unitOfWork.Anomalies.GetAllAsync();
        var activeAlerts = allAnomalies.Count(a => !a.IsResolved);

        var responseTimeMetrics = metrics.Where(m => m.MetricName == "ResponseTime").ToList();
        var avgResponseTime = responseTimeMetrics.Any() ? responseTimeMetrics.Average(m => m.Value) : 0;

        var uptimeMetrics = metrics.Where(m => m.MetricName == "Uptime").ToList();
        var avgUptime = uptimeMetrics.Any() ? uptimeMetrics.Average(m => m.Value) : 0;

        var requestMetrics = metrics.Where(m => m.MetricName == "RequestCount").ToList();
        var totalRequests = requestMetrics.Any() ? (long)requestMetrics.Sum(m => m.Value) : 0;

        var errorMetrics = metrics.Where(m => m.MetricName == "ErrorCount").ToList();
        var totalErrors = errorMetrics.Any() ? (int)errorMetrics.Sum(m => m.Value) : 0;

        return new DashboardSummaryDto
        {
            TotalServers = servers,
            ActiveAlerts = activeAlerts,
            AverageResponseTime = Math.Round(avgResponseTime, 2),
            SystemAvailability = Math.Round(avgUptime, 2),
            TotalRequests = totalRequests,
            TotalErrors = totalErrors
        };
    }

    private List<TimeSeriesDataDto> GetResponseTimeTrend(List<SystemMetric> metrics, int hours)
    {
        var responseMetrics = metrics
            .Where(m => m.MetricName == "ResponseTime")
            .OrderBy(m => m.Timestamp)
            .ToList();

        var hourlyData = responseMetrics
            .GroupBy(m => new DateTime(m.Timestamp.Year, m.Timestamp.Month, m.Timestamp.Day, m.Timestamp.Hour, 0, 0))
            .Select(g => new TimeSeriesDataDto
            {
                Timestamp = g.Key,
                Value = Math.Round(g.Average(m => m.Value), 2),
                Label = g.Key.ToString("HH:mm")
            })
            .OrderBy(d => d.Timestamp)
            .ToList();

        return hourlyData;
    }

    private List<TimeSeriesDataDto> GetThroughputTrend(List<SystemMetric> metrics, int hours)
    {
        var requestMetrics = metrics
            .Where(m => m.MetricName == "RequestCount")
            .OrderBy(m => m.Timestamp)
            .ToList();

        var hourlyData = requestMetrics
            .GroupBy(m => new DateTime(m.Timestamp.Year, m.Timestamp.Month, m.Timestamp.Day, m.Timestamp.Hour, 0, 0))
            .Select(g => new TimeSeriesDataDto
            {
                Timestamp = g.Key,
                Value = Math.Round(g.Sum(m => m.Value), 2),
                Label = g.Key.ToString("HH:mm")
            })
            .OrderBy(d => d.Timestamp)
            .ToList();

        return hourlyData;
    }

    private List<MetricDistributionDto> GetMetricDistribution(List<SystemMetric> metrics)
    {
        var distribution = metrics
            .GroupBy(m => m.MetricName)
            .Select(g => new MetricDistributionDto
            {
                MetricName = g.Key,
                Value = Math.Round(g.Average(m => m.Value), 2),
                Category = GetMetricCategory(g.Key)
            })
            .ToList();

        return distribution;
    }

    private string GetMetricCategory(string metricName)
    {
        return metricName switch
        {
            "CPUUsage" or "MemoryUsage" or "DiskIO" => "Infrastructure",
            "ResponseTime" or "RequestCount" or "ErrorCount" => "Application",
            "NetworkThroughput" or "ActiveConnections" => "Network",
            "Uptime" => "Availability",
            _ => "Other"
        };
    }
}
