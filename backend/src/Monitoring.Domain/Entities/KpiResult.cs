using Monitoring.Domain.Enums;

namespace Monitoring.Domain.Entities;

public class KpiResult
{
    public int Id { get; set; }
    public string KpiName { get; set; } = string.Empty;
    public double CalculatedValue { get; set; }
    public double? TargetValue { get; set; }
    public KpiStatus Status { get; set; }
    public DateTime CalculatedAt { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? Description { get; set; }
}
