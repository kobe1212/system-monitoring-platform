namespace Monitoring.Domain.Entities;

public class SystemMetric
{
    public int Id { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Tags { get; set; }
}
