using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;
using Monitoring.Domain.Entities;

namespace Monitoring.Application.Services;

public class MetricService : IMetricService
{
    private readonly IUnitOfWork _unitOfWork;

    public MetricService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<SystemMetricDto> CreateMetricAsync(CreateMetricDto createMetricDto)
    {
        var metric = new SystemMetric
        {
            MetricName = createMetricDto.MetricName,
            Value = createMetricDto.Value,
            Unit = createMetricDto.Unit,
            Source = createMetricDto.Source,
            Tags = createMetricDto.Tags,
            Timestamp = DateTime.UtcNow
        };

        var createdMetric = await _unitOfWork.SystemMetrics.AddAsync(metric);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(createdMetric);
    }

    public async Task<IEnumerable<SystemMetricDto>> GetMetricsAsync(MetricQueryDto query)
    {
        var metrics = await _unitOfWork.SystemMetrics.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(query.MetricName))
        {
            metrics = metrics.Where(m => m.MetricName.Contains(query.MetricName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Source))
        {
            metrics = metrics.Where(m => m.Source.Contains(query.Source, StringComparison.OrdinalIgnoreCase));
        }

        if (query.StartDate.HasValue)
        {
            metrics = metrics.Where(m => m.Timestamp >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            metrics = metrics.Where(m => m.Timestamp <= query.EndDate.Value);
        }

        var orderedMetrics = metrics.OrderByDescending(m => m.Timestamp);
        var pagedMetrics = orderedMetrics
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);

        return pagedMetrics.Select(MapToDto);
    }

    public async Task<SystemMetricDto?> GetMetricByIdAsync(int id)
    {
        var metric = await _unitOfWork.SystemMetrics.GetByIdAsync(id);
        return metric != null ? MapToDto(metric) : null;
    }

    public async Task<IEnumerable<SystemMetricDto>> GetRecentMetricsAsync(int count = 100)
    {
        var metrics = await _unitOfWork.SystemMetrics.GetAllAsync();
        var recentMetrics = metrics
            .OrderByDescending(m => m.Timestamp)
            .Take(count);

        return recentMetrics.Select(MapToDto);
    }

    private static SystemMetricDto MapToDto(SystemMetric metric)
    {
        return new SystemMetricDto
        {
            Id = metric.Id,
            MetricName = metric.MetricName,
            Value = metric.Value,
            Unit = metric.Unit,
            Timestamp = metric.Timestamp,
            Source = metric.Source,
            Tags = metric.Tags
        };
    }
}
