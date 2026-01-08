using Microsoft.AspNetCore.Mvc;
using Monitoring.Application.Interfaces;
using Monitoring.Application.DTOs;

namespace Monitoring.API.Controllers;

/// <summary>
/// Advanced trend analysis endpoints demonstrating statistical thinking and pattern recognition
/// Aligned with Intel Foundry requirements for seasonality detection, variance analysis, and forecasting
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TrendAnalysisController : ControllerBase
{
    private readonly ITrendAnalysisService _trendAnalysisService;
    private readonly ILogger<TrendAnalysisController> _logger;

    public TrendAnalysisController(
        ITrendAnalysisService trendAnalysisService,
        ILogger<TrendAnalysisController> logger)
    {
        _trendAnalysisService = trendAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Detects seasonality patterns in metrics (hourly, daily, weekly)
    /// Demonstrates: Pattern recognition and understanding of cyclical behavior
    /// </summary>
    [HttpGet("seasonality/{metricType}")]
    [ProducesResponseType(typeof(SeasonalityAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeasonalityAnalysisDto>> DetectSeasonality(
        string metricType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Detecting seasonality for {MetricType} from {Start} to {End}", 
                metricType, start, end);

            var result = await _trendAnalysisService.DetectSeasonalityAsync(metricType, start, end);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting seasonality for {MetricType}", metricType);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes variance and standard deviation for metric stability
    /// Demonstrates: Statistical thinking and understanding of variance
    /// </summary>
    [HttpGet("variance/{metricType}")]
    [ProducesResponseType(typeof(VarianceAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VarianceAnalysisDto>> AnalyzeVariance(
        string metricType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Analyzing variance for {MetricType} from {Start} to {End}", 
                metricType, start, end);

            var result = await _trendAnalysisService.AnalyzeVarianceAsync(metricType, start, end);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing variance for {MetricType}", metricType);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tests if a change is statistically significant or just noise
    /// Demonstrates: Ability to distinguish between real changes and random variance
    /// </summary>
    [HttpPost("statistical-significance")]
    [ProducesResponseType(typeof(StatisticalSignificanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StatisticalSignificanceDto>> TestStatisticalSignificance(
        [FromBody] StatisticalSignificanceRequest request)
    {
        try
        {
            _logger.LogInformation("Testing statistical significance for {MetricType}", request.MetricType);

            var result = await _trendAnalysisService.TestStatisticalSignificanceAsync(
                request.MetricType,
                request.BaselinePeriodStart,
                request.BaselinePeriodEnd,
                request.ComparisonPeriodStart,
                request.ComparisonPeriodEnd);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing statistical significance for {MetricType}", request.MetricType);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Identifies trends (upward, downward, stable) with confidence levels
    /// Demonstrates: Trend analysis to distinguish between trends and one-off issues
    /// </summary>
    [HttpGet("trend/{metricType}")]
    [ProducesResponseType(typeof(TrendAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrendAnalysisDto>> AnalyzeTrend(
        string metricType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Analyzing trend for {MetricType} from {Start} to {End}", 
                metricType, start, end);

            var result = await _trendAnalysisService.AnalyzeTrendAsync(metricType, start, end);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing trend for {MetricType}", metricType);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Forecasts future values based on historical trends
    /// Demonstrates: Predictive analysis and leading indicator identification
    /// </summary>
    [HttpGet("forecast/{metricType}")]
    [ProducesResponseType(typeof(ForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ForecastDto>> ForecastMetric(
        string metricType,
        [FromQuery] int hoursAhead = 24)
    {
        try
        {
            if (hoursAhead < 1 || hoursAhead > 168) // Max 1 week ahead
            {
                return BadRequest(new { error = "hoursAhead must be between 1 and 168" });
            }

            _logger.LogInformation("Forecasting {MetricType} for {Hours} hours ahead", 
                metricType, hoursAhead);

            var result = await _trendAnalysisService.ForecastMetricAsync(metricType, hoursAhead);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting {MetricType}", metricType);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Classifies anomaly as one-off spike or sustained issue
    /// Demonstrates: Ability to distinguish between trends and one-off issues
    /// </summary>
    [HttpGet("classify-anomaly/{anomalyId}")]
    [ProducesResponseType(typeof(AnomalyClassificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnomalyClassificationDto>> ClassifyAnomaly(int anomalyId)
    {
        try
        {
            _logger.LogInformation("Classifying anomaly {AnomalyId}", anomalyId);

            var result = await _trendAnalysisService.ClassifyAnomalyTypeAsync(anomalyId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Anomaly {anomalyId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying anomaly {AnomalyId}", anomalyId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets comprehensive trend report for dashboard
    /// Demonstrates: Holistic system analysis and stakeholder communication
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(TrendReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrendReportDto>> GetTrendReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Generating trend report from {Start} to {End}", start, end);

            var result = await _trendAnalysisService.GetTrendReportAsync(start, end);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating trend report");
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for statistical significance testing
/// </summary>
public class StatisticalSignificanceRequest
{
    public string MetricType { get; set; } = string.Empty;
    public DateTime BaselinePeriodStart { get; set; }
    public DateTime BaselinePeriodEnd { get; set; }
    public DateTime ComparisonPeriodStart { get; set; }
    public DateTime ComparisonPeriodEnd { get; set; }
}
