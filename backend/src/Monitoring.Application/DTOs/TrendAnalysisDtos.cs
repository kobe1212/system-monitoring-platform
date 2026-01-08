namespace Monitoring.Application.DTOs;

/// <summary>
/// Seasonality analysis results showing patterns in data
/// </summary>
public class SeasonalityAnalysisDto
{
    public string MetricType { get; set; } = string.Empty;
    public bool HasSeasonality { get; set; }
    public string SeasonalityType { get; set; } = string.Empty; // Hourly, Daily, Weekly
    public Dictionary<string, double> PeakPeriods { get; set; } = new();
    public Dictionary<string, double> LowPeriods { get; set; } = new();
    public double SeasonalityStrength { get; set; } // 0-1 confidence
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Variance and standard deviation analysis
/// </summary>
public class VarianceAnalysisDto
{
    public string MetricType { get; set; } = string.Empty;
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double Variance { get; set; }
    public double CoefficientOfVariation { get; set; } // CV = StdDev / Mean
    public string StabilityAssessment { get; set; } = string.Empty; // Stable, Moderate, Volatile
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double Range { get; set; }
    public int SampleSize { get; set; }
}

/// <summary>
/// Statistical significance test results (T-test)
/// </summary>
public class StatisticalSignificanceDto
{
    public string MetricType { get; set; } = string.Empty;
    public double BaselineMean { get; set; }
    public double ComparisonMean { get; set; }
    public double PercentageChange { get; set; }
    public double TStatistic { get; set; }
    public double PValue { get; set; }
    public bool IsStatisticallySignificant { get; set; } // p-value < 0.05
    public string Conclusion { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Trend direction and strength analysis
/// </summary>
public class TrendAnalysisDto
{
    public string MetricType { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = string.Empty; // Upward, Downward, Stable
    public double TrendStrength { get; set; } // 0-1 confidence
    public double SlopePerHour { get; set; }
    public double RSquared { get; set; } // Goodness of fit
    public bool IsSustainedTrend { get; set; }
    public DateTime TrendStartDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<TrendDataPoint> DataPoints { get; set; } = new();
}

public class TrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public double ActualValue { get; set; }
    public double TrendLineValue { get; set; }
}

/// <summary>
/// Forecast predictions with confidence intervals
/// </summary>
public class ForecastDto
{
    public string MetricType { get; set; } = string.Empty;
    public List<ForecastPoint> Predictions { get; set; } = new();
    public double ConfidenceLevel { get; set; }
    public string ForecastMethod { get; set; } = string.Empty; // Linear, Moving Average, etc.
    public string Warning { get; set; } = string.Empty;
}

public class ForecastPoint
{
    public DateTime Timestamp { get; set; }
    public double PredictedValue { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
}

/// <summary>
/// Classification of anomaly as one-off spike or sustained issue
/// </summary>
public class AnomalyClassificationDto
{
    public int AnomalyId { get; set; }
    public string Classification { get; set; } = string.Empty; // OneOffSpike, SustainedIssue, RecurringPattern
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public bool RequiresImmediateAction { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}

/// <summary>
/// Comprehensive trend report for dashboard
/// </summary>
public class TrendReportDto
{
    public DateTime ReportGeneratedAt { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<MetricTrendSummary> MetricTrends { get; set; } = new();
    public List<string> KeyFindings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public int TotalAnomaliesDetected { get; set; }
    public int OneOffSpikes { get; set; }
    public int SustainedIssues { get; set; }
}

public class MetricTrendSummary
{
    public string MetricType { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = string.Empty;
    public double ChangePercentage { get; set; }
    public bool HasSeasonality { get; set; }
    public string StabilityAssessment { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty; // Healthy, Warning, Critical
}
