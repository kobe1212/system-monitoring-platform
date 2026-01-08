using System.ComponentModel.DataAnnotations;

namespace Monitoring.Application.DTOs;

public class CreateMetricDto
{
    [Required(ErrorMessage = "Metric name is required")]
    [StringLength(100, MinimumLength = 1)]
    public string MetricName { get; set; } = string.Empty;

    [Required]
    [Range(-1e10, 1e10, ErrorMessage = "Value must be within valid range")]
    public double Value { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Source is required")]
    [StringLength(100)]
    public string Source { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Tags { get; set; }
}
