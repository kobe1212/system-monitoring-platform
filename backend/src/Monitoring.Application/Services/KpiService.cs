using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;
using Monitoring.Domain.Entities;
using Monitoring.Domain.Enums;

namespace Monitoring.Application.Services;

public class KpiService : IKpiService
{
    private readonly IUnitOfWork _unitOfWork;

    public KpiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<KpiResultDto>> GetAllKpisAsync()
    {
        var kpis = await _unitOfWork.KpiResults.GetAllAsync();
        return kpis.OrderByDescending(k => k.CalculatedAt).Select(MapToDto);
    }

    public async Task<IEnumerable<KpiResultDto>> GetKpisByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var kpis = await _unitOfWork.KpiResults.FindAsync(k => 
            k.CalculatedAt >= startDate && k.CalculatedAt <= endDate);
        
        return kpis.OrderByDescending(k => k.CalculatedAt).Select(MapToDto);
    }

    public async Task CalculateKpisAsync()
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);

        var recentMetrics = await _unitOfWork.SystemMetrics.FindAsync(m => m.Timestamp >= last24Hours);
        var metricsList = recentMetrics.ToList();

        if (!metricsList.Any())
        {
            return;
        }

        await CalculateAverageResponseTimeKpi(metricsList, last24Hours, now);
        await CalculateThroughputKpi(metricsList, last24Hours, now);
        await CalculateErrorRateKpi(metricsList, last24Hours, now);
        await CalculateSystemAvailabilityKpi(metricsList, last24Hours, now);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task CalculateAverageResponseTimeKpi(List<SystemMetric> metrics, DateTime periodStart, DateTime periodEnd)
    {
        var responseTimeMetrics = metrics
            .Where(m => m.MetricName.Equals("ResponseTime", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!responseTimeMetrics.Any()) return;

        var averageResponseTime = responseTimeMetrics.Average(m => m.Value);
        const double targetResponseTime = 200.0;

        var status = DetermineKpiStatus(averageResponseTime, targetResponseTime, isLowerBetter: true);

        var kpi = new KpiResult
        {
            KpiName = "Average Response Time",
            CalculatedValue = Math.Round(averageResponseTime, 2),
            TargetValue = targetResponseTime,
            Status = status,
            CalculatedAt = DateTime.UtcNow,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Description = $"Average response time over the last 24 hours. Target: {targetResponseTime}ms"
        };

        await _unitOfWork.KpiResults.AddAsync(kpi);
    }

    private async Task CalculateThroughputKpi(List<SystemMetric> metrics, DateTime periodStart, DateTime periodEnd)
    {
        var requestMetrics = metrics
            .Where(m => m.MetricName.Equals("RequestCount", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!requestMetrics.Any()) return;

        var totalRequests = requestMetrics.Sum(m => m.Value);
        var hoursInPeriod = (periodEnd - periodStart).TotalHours;
        var requestsPerHour = totalRequests / hoursInPeriod;
        const double targetThroughput = 1000.0;

        var status = DetermineKpiStatus(requestsPerHour, targetThroughput, isLowerBetter: false);

        var kpi = new KpiResult
        {
            KpiName = "Throughput",
            CalculatedValue = Math.Round(requestsPerHour, 2),
            TargetValue = targetThroughput,
            Status = status,
            CalculatedAt = DateTime.UtcNow,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Description = $"Requests per hour over the last 24 hours. Target: {targetThroughput} req/hr"
        };

        await _unitOfWork.KpiResults.AddAsync(kpi);
    }

    private async Task CalculateErrorRateKpi(List<SystemMetric> metrics, DateTime periodStart, DateTime periodEnd)
    {
        var errorMetrics = metrics
            .Where(m => m.MetricName.Equals("ErrorCount", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var requestMetrics = metrics
            .Where(m => m.MetricName.Equals("RequestCount", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!requestMetrics.Any()) return;

        var totalErrors = errorMetrics.Sum(m => m.Value);
        var totalRequests = requestMetrics.Sum(m => m.Value);
        var errorRate = totalRequests > 0 ? (totalErrors / totalRequests) * 100 : 0;
        const double targetErrorRate = 1.0;

        var status = DetermineKpiStatus(errorRate, targetErrorRate, isLowerBetter: true);

        var kpi = new KpiResult
        {
            KpiName = "Error Rate",
            CalculatedValue = Math.Round(errorRate, 2),
            TargetValue = targetErrorRate,
            Status = status,
            CalculatedAt = DateTime.UtcNow,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Description = $"Error rate percentage over the last 24 hours. Target: <{targetErrorRate}%"
        };

        await _unitOfWork.KpiResults.AddAsync(kpi);
    }

    private async Task CalculateSystemAvailabilityKpi(List<SystemMetric> metrics, DateTime periodStart, DateTime periodEnd)
    {
        var uptimeMetrics = metrics
            .Where(m => m.MetricName.Equals("Uptime", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!uptimeMetrics.Any()) return;

        var averageUptime = uptimeMetrics.Average(m => m.Value);
        const double targetUptime = 99.9;

        var status = DetermineKpiStatus(averageUptime, targetUptime, isLowerBetter: false);

        var kpi = new KpiResult
        {
            KpiName = "System Availability",
            CalculatedValue = Math.Round(averageUptime, 2),
            TargetValue = targetUptime,
            Status = status,
            CalculatedAt = DateTime.UtcNow,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Description = $"System uptime percentage over the last 24 hours. Target: {targetUptime}%"
        };

        await _unitOfWork.KpiResults.AddAsync(kpi);
    }

    private static KpiStatus DetermineKpiStatus(double actualValue, double targetValue, bool isLowerBetter)
    {
        var percentageOfTarget = (actualValue / targetValue) * 100;

        if (isLowerBetter)
        {
            return percentageOfTarget switch
            {
                <= 80 => KpiStatus.AboveTarget,
                <= 100 => KpiStatus.OnTarget,
                <= 150 => KpiStatus.BelowTarget,
                _ => KpiStatus.Critical
            };
        }
        else
        {
            return percentageOfTarget switch
            {
                >= 100 => KpiStatus.AboveTarget,
                >= 80 => KpiStatus.OnTarget,
                >= 50 => KpiStatus.BelowTarget,
                _ => KpiStatus.Critical
            };
        }
    }

    private static KpiResultDto MapToDto(KpiResult kpi)
    {
        double? percentageOfTarget = kpi.TargetValue.HasValue && kpi.TargetValue.Value > 0
            ? Math.Round((kpi.CalculatedValue / kpi.TargetValue.Value) * 100, 2)
            : null;

        return new KpiResultDto
        {
            Id = kpi.Id,
            KpiName = kpi.KpiName,
            CalculatedValue = kpi.CalculatedValue,
            TargetValue = kpi.TargetValue,
            Status = kpi.Status,
            StatusText = kpi.Status.ToString(),
            CalculatedAt = kpi.CalculatedAt,
            PeriodStart = kpi.PeriodStart,
            PeriodEnd = kpi.PeriodEnd,
            Description = kpi.Description,
            PercentageOfTarget = percentageOfTarget
        };
    }
}
