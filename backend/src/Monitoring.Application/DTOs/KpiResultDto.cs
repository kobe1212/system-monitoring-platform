using Monitoring.Domain.Enums;

namespace Monitoring.Application.DTOs;

public class KpiResultDto
{
    public int Id { get; set; }
    public string KpiName { get; set; } = string.Empty;
    public double CalculatedValue { get; set; }
    public double? TargetValue { get; set; }
    public KpiStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? Description { get; set; }
    public double? PercentageOfTarget { get; set; }
}
