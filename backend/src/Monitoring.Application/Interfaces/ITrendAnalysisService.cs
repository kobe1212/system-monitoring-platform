using Monitoring.Application.DTOs;

namespace Monitoring.Application.Interfaces;

/// <summary>
/// Service for advanced trend analysis, seasonality detection, and statistical analysis
/// Demonstrates: Statistical thinking, variance analysis, trend vs one-off issue distinction
/// </summary>
public interface ITrendAnalysisService
{
    /// <summary>
    /// Detects seasonality patterns in metrics (hourly, daily, weekly)
    /// </summary>
    Task<SeasonalityAnalysisDto> DetectSeasonalityAsync(string metricType, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Analyzes variance and standard deviation for metric stability
    /// </summary>
    Task<VarianceAnalysisDto> AnalyzeVarianceAsync(string metricType, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Determines if a change is statistically significant or just noise
    /// </summary>
    Task<StatisticalSignificanceDto> TestStatisticalSignificanceAsync(
        string metricType, 
        DateTime baselinePeriodStart, 
        DateTime baselinePeriodEnd,
        DateTime comparisonPeriodStart,
        DateTime comparisonPeriodEnd);

    /// <summary>
    /// Identifies trends (upward, downward, stable) with confidence levels
    /// </summary>
    Task<TrendAnalysisDto> AnalyzeTrendAsync(string metricType, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Forecasts future values based on historical trends
    /// </summary>
    Task<ForecastDto> ForecastMetricAsync(string metricType, int hoursAhead);

    /// <summary>
    /// Distinguishes between one-off spikes and sustained trends
    /// </summary>
    Task<AnomalyClassificationDto> ClassifyAnomalyTypeAsync(int anomalyId);

    /// <summary>
    /// Gets comprehensive trend report for dashboard
    /// </summary>
    Task<TrendReportDto> GetTrendReportAsync(DateTime startDate, DateTime endDate);
}
