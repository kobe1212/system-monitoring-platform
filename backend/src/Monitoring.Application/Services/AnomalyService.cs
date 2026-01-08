using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;
using Monitoring.Domain.Entities;
using Monitoring.Domain.Enums;

namespace Monitoring.Application.Services;

public class AnomalyService : IAnomalyService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnomalyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<AnomalyDto>> GetAllAnomaliesAsync()
    {
        var anomalies = await _unitOfWork.Anomalies.GetAllAsync();
        return anomalies.OrderByDescending(a => a.DetectedAt).Select(MapToDto);
    }

    public async Task<IEnumerable<AnomalyDto>> GetUnresolvedAnomaliesAsync()
    {
        var anomalies = await _unitOfWork.Anomalies.FindAsync(a => !a.IsResolved);
        return anomalies.OrderByDescending(a => a.DetectedAt).Select(MapToDto);
    }

    public async Task DetectAnomaliesAsync()
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);

        var recentMetrics = await _unitOfWork.SystemMetrics.FindAsync(m => m.Timestamp >= last24Hours);
        var metricGroups = recentMetrics.GroupBy(m => m.MetricName);

        foreach (var group in metricGroups)
        {
            await DetectAnomaliesForMetric(group.Key, group.ToList());
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task DetectAnomaliesForMetric(string metricName, List<SystemMetric> metrics)
    {
        if (metrics.Count < 10) return;

        var values = metrics.Select(m => m.Value).ToList();
        var mean = values.Average();
        var stdDev = CalculateStandardDeviation(values, mean);

        var latestMetric = metrics.OrderByDescending(m => m.Timestamp).First();
        var zScore = Math.Abs((latestMetric.Value - mean) / stdDev);

        if (zScore > 2.0)
        {
            var deviation = ((latestMetric.Value - mean) / mean) * 100;
            var severity = DetermineSeverity(zScore);

            var existingAnomaly = await _unitOfWork.Anomalies.FindAsync(a =>
                a.MetricName == metricName &&
                !a.IsResolved &&
                a.DetectedAt >= DateTime.UtcNow.AddMinutes(-30));

            if (!existingAnomaly.Any())
            {
                var anomaly = new Anomaly
                {
                    MetricName = metricName,
                    DetectedValue = latestMetric.Value,
                    ExpectedValue = mean,
                    Deviation = Math.Round(deviation, 2),
                    Severity = severity,
                    DetectedAt = DateTime.UtcNow,
                    IsResolved = false,
                    Description = $"Detected anomaly: value {latestMetric.Value:F2} deviates {Math.Abs(deviation):F2}% from expected {mean:F2}"
                };

                await _unitOfWork.Anomalies.AddAsync(anomaly);
            }
        }
    }

    public async Task<bool> ResolveAnomalyAsync(int anomalyId)
    {
        var anomaly = await _unitOfWork.Anomalies.GetByIdAsync(anomalyId);
        
        if (anomaly == null || anomaly.IsResolved)
        {
            return false;
        }

        anomaly.IsResolved = true;
        anomaly.ResolvedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static double CalculateStandardDeviation(List<double> values, double mean)
    {
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        var variance = sumOfSquares / values.Count;
        return Math.Sqrt(variance);
    }

    private static AnomalySeverity DetermineSeverity(double zScore)
    {
        return zScore switch
        {
            >= 4.0 => AnomalySeverity.Critical,
            >= 3.0 => AnomalySeverity.High,
            >= 2.5 => AnomalySeverity.Medium,
            _ => AnomalySeverity.Low
        };
    }

    private static AnomalyDto MapToDto(Anomaly anomaly)
    {
        return new AnomalyDto
        {
            Id = anomaly.Id,
            MetricName = anomaly.MetricName,
            DetectedValue = anomaly.DetectedValue,
            ExpectedValue = anomaly.ExpectedValue,
            Deviation = anomaly.Deviation,
            Severity = anomaly.Severity,
            SeverityText = anomaly.Severity.ToString(),
            DetectedAt = anomaly.DetectedAt,
            IsResolved = anomaly.IsResolved,
            ResolvedAt = anomaly.ResolvedAt,
            Description = anomaly.Description
        };
    }
}
