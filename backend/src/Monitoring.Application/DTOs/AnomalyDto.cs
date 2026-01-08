using Monitoring.Domain.Enums;

namespace Monitoring.Application.DTOs;

public class AnomalyDto
{
    public int Id { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double DetectedValue { get; set; }
    public double ExpectedValue { get; set; }
    public double Deviation { get; set; }
    public AnomalySeverity Severity { get; set; }
    public string SeverityText { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Description { get; set; }
}
