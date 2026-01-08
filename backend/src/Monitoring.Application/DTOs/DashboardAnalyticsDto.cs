namespace Monitoring.Application.DTOs;

public class DashboardAnalyticsDto
{
    public DashboardSummaryDto Summary { get; set; } = new();
    public List<TimeSeriesDataDto> ResponseTimeTrend { get; set; } = new();
    public List<TimeSeriesDataDto> ThroughputTrend { get; set; } = new();
    public List<ServerMetricsDto> ServerMetrics { get; set; } = new();
    public List<MetricDistributionDto> MetricDistribution { get; set; } = new();
}

public class DashboardSummaryDto
{
    public int TotalServers { get; set; }
    public int ActiveAlerts { get; set; }
    public double AverageResponseTime { get; set; }
    public double SystemAvailability { get; set; }
    public long TotalRequests { get; set; }
    public int TotalErrors { get; set; }
}

public class TimeSeriesDataDto
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Label { get; set; }
}

public class ServerMetricsDto
{
    public string ServerName { get; set; } = string.Empty;
    public double CPUUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double ResponseTime { get; set; }
    public int RequestCount { get; set; }
    public string Status { get; set; } = "Healthy";
}

public class MetricDistributionDto
{
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Category { get; set; } = string.Empty;
}
