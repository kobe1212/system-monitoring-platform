namespace Monitoring.Application.DTOs;

public class MetricQueryDto
{
    public string? MetricName { get; set; }
    public string? Source { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
