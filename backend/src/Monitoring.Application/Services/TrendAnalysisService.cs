using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;
using Monitoring.Domain.Entities;

namespace Monitoring.Application.Services;

/// <summary>
/// Advanced trend analysis service demonstrating statistical thinking and pattern recognition
/// Implements Intel Foundry requirements: seasonality detection, variance analysis, trend identification
/// </summary>
public class TrendAnalysisService : ITrendAnalysisService
{
    private readonly IUnitOfWork _unitOfWork;

    public TrendAnalysisService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SeasonalityAnalysisDto> DetectSeasonalityAsync(string metricType, DateTime startDate, DateTime endDate)
    {
        var metrics = await GetMetricsByTypeAndPeriod(metricType, startDate, endDate);
        
        if (metrics.Count < 24) // Need at least 24 hours of data
        {
            return new SeasonalityAnalysisDto
            {
                MetricType = metricType,
                HasSeasonality = false,
                Description = "Insufficient data for seasonality analysis"
            };
        }

        // Analyze hourly patterns
        var hourlyAverages = metrics
            .GroupBy(m => m.Timestamp.Hour)
            .Select(g => new { Hour = g.Key, Average = g.Average(m => m.Value) })
            .OrderBy(x => x.Hour)
            .ToList();

        var overallMean = hourlyAverages.Average(x => x.Average);
        var peakThreshold = overallMean * 1.2; // 20% above mean
        var lowThreshold = overallMean * 0.8;  // 20% below mean

        var peakPeriods = hourlyAverages
            .Where(x => x.Average > peakThreshold)
            .ToDictionary(x => $"{x.Hour:D2}:00", x => x.Average);

        var lowPeriods = hourlyAverages
            .Where(x => x.Average < lowThreshold)
            .ToDictionary(x => $"{x.Hour:D2}:00", x => x.Average);

        // Calculate seasonality strength using coefficient of variation
        var stdDev = CalculateStandardDeviation(hourlyAverages.Select(x => x.Average).ToList());
        var cv = stdDev / overallMean;
        var seasonalityStrength = Math.Min(cv * 2, 1.0); // Normalize to 0-1

        var hasSeasonality = peakPeriods.Any() && lowPeriods.Any() && seasonalityStrength > 0.15;

        return new SeasonalityAnalysisDto
        {
            MetricType = metricType,
            HasSeasonality = hasSeasonality,
            SeasonalityType = "Hourly",
            PeakPeriods = peakPeriods,
            LowPeriods = lowPeriods,
            SeasonalityStrength = Math.Round(seasonalityStrength, 2),
            Description = hasSeasonality 
                ? $"Clear seasonality detected with {peakPeriods.Count} peak periods and {lowPeriods.Count} low periods"
                : "No significant seasonality pattern detected"
        };
    }

    public async Task<VarianceAnalysisDto> AnalyzeVarianceAsync(string metricType, DateTime startDate, DateTime endDate)
    {
        var metrics = await GetMetricsByTypeAndPeriod(metricType, startDate, endDate);
        
        if (!metrics.Any())
        {
            throw new InvalidOperationException($"No metrics found for {metricType} in the specified period");
        }

        var values = metrics.Select(m => m.Value).ToList();
        var mean = values.Average();
        var stdDev = CalculateStandardDeviation(values);
        var variance = stdDev * stdDev;
        var cv = mean > 0 ? (stdDev / mean) : 0;
        var min = values.Min();
        var max = values.Max();
        var range = max - min;

        // Assess stability based on coefficient of variation
        string stability;
        if (cv < 0.15) stability = "Stable";
        else if (cv < 0.30) stability = "Moderate";
        else stability = "Volatile";

        return new VarianceAnalysisDto
        {
            MetricType = metricType,
            Mean = Math.Round(mean, 2),
            StandardDeviation = Math.Round(stdDev, 2),
            Variance = Math.Round(variance, 2),
            CoefficientOfVariation = Math.Round(cv, 3),
            StabilityAssessment = stability,
            MinValue = Math.Round(min, 2),
            MaxValue = Math.Round(max, 2),
            Range = Math.Round(range, 2),
            SampleSize = values.Count
        };
    }

    public async Task<StatisticalSignificanceDto> TestStatisticalSignificanceAsync(
        string metricType,
        DateTime baselinePeriodStart,
        DateTime baselinePeriodEnd,
        DateTime comparisonPeriodStart,
        DateTime comparisonPeriodEnd)
    {
        var baselineMetrics = await GetMetricsByTypeAndPeriod(metricType, baselinePeriodStart, baselinePeriodEnd);
        var comparisonMetrics = await GetMetricsByTypeAndPeriod(metricType, comparisonPeriodStart, comparisonPeriodEnd);

        if (!baselineMetrics.Any() || !comparisonMetrics.Any())
        {
            throw new InvalidOperationException("Insufficient data for statistical significance testing");
        }

        var baselineValues = baselineMetrics.Select(m => m.Value).ToList();
        var comparisonValues = comparisonMetrics.Select(m => m.Value).ToList();

        var baselineMean = baselineValues.Average();
        var comparisonMean = comparisonValues.Average();
        var percentageChange = ((comparisonMean - baselineMean) / baselineMean) * 100;

        // Perform Welch's t-test (doesn't assume equal variances)
        var (tStatistic, pValue) = PerformWelchTTest(baselineValues, comparisonValues);
        var isSignificant = pValue < 0.05; // 95% confidence level

        string conclusion;
        string recommendation;

        if (!isSignificant)
        {
            conclusion = $"The change of {percentageChange:F2}% is NOT statistically significant (p={pValue:F4}). This appears to be normal variance.";
            recommendation = "Continue monitoring. No immediate action required.";
        }
        else if (percentageChange > 0)
        {
            conclusion = $"The increase of {percentageChange:F2}% IS statistically significant (p={pValue:F4}). This represents a real change, not random variance.";
            recommendation = percentageChange > 20 
                ? "Investigate root cause immediately. Significant degradation detected."
                : "Monitor closely. Trend may continue if not addressed.";
        }
        else
        {
            conclusion = $"The decrease of {Math.Abs(percentageChange):F2}% IS statistically significant (p={pValue:F4}). This represents a real improvement.";
            recommendation = "Document changes that led to improvement for future reference.";
        }

        return new StatisticalSignificanceDto
        {
            MetricType = metricType,
            BaselineMean = Math.Round(baselineMean, 2),
            ComparisonMean = Math.Round(comparisonMean, 2),
            PercentageChange = Math.Round(percentageChange, 2),
            TStatistic = Math.Round(tStatistic, 4),
            PValue = Math.Round(pValue, 4),
            IsStatisticallySignificant = isSignificant,
            Conclusion = conclusion,
            Recommendation = recommendation
        };
    }

    public async Task<TrendAnalysisDto> AnalyzeTrendAsync(string metricType, DateTime startDate, DateTime endDate)
    {
        var metrics = await GetMetricsByTypeAndPeriod(metricType, startDate, endDate);
        
        if (metrics.Count < 10)
        {
            throw new InvalidOperationException("Insufficient data points for trend analysis (minimum 10 required)");
        }

        // Prepare data for linear regression
        var orderedMetrics = metrics.OrderBy(m => m.Timestamp).ToList();
        var dataPoints = new List<TrendDataPoint>();
        var xValues = new List<double>();
        var yValues = new List<double>();

        for (int i = 0; i < orderedMetrics.Count; i++)
        {
            xValues.Add(i);
            yValues.Add(orderedMetrics[i].Value);
        }

        // Calculate linear regression
        var (slope, intercept, rSquared) = CalculateLinearRegression(xValues, yValues);

        // Generate trend line values
        for (int i = 0; i < orderedMetrics.Count; i++)
        {
            dataPoints.Add(new TrendDataPoint
            {
                Timestamp = orderedMetrics[i].Timestamp,
                ActualValue = Math.Round(orderedMetrics[i].Value, 2),
                TrendLineValue = Math.Round(slope * i + intercept, 2)
            });
        }

        // Determine trend direction and strength
        string trendDirection;
        double trendStrength = Math.Abs(rSquared);
        
        if (Math.Abs(slope) < 0.01)
        {
            trendDirection = "Stable";
        }
        else if (slope > 0)
        {
            trendDirection = "Upward";
        }
        else
        {
            trendDirection = "Downward";
        }

        // Calculate slope per hour
        var totalHours = (endDate - startDate).TotalHours;
        var slopePerHour = (slope * orderedMetrics.Count) / totalHours;

        // Determine if trend is sustained (R² > 0.7 indicates strong correlation)
        var isSustained = rSquared > 0.7;

        var description = $"{trendDirection} trend detected with {(isSustained ? "strong" : "weak")} correlation (R²={rSquared:F3}). ";
        description += trendDirection switch
        {
            "Upward" => $"Metric increasing at {Math.Abs(slopePerHour):F2} units/hour.",
            "Downward" => $"Metric decreasing at {Math.Abs(slopePerHour):F2} units/hour.",
            _ => "Metric remains relatively stable."
        };

        return new TrendAnalysisDto
        {
            MetricType = metricType,
            TrendDirection = trendDirection,
            TrendStrength = Math.Round(trendStrength, 3),
            SlopePerHour = Math.Round(slopePerHour, 4),
            RSquared = Math.Round(rSquared, 3),
            IsSustainedTrend = isSustained,
            TrendStartDate = startDate,
            Description = description,
            DataPoints = dataPoints
        };
    }

    public async Task<ForecastDto> ForecastMetricAsync(string metricType, int hoursAhead)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-7); // Use last 7 days for forecast
        
        var trendAnalysis = await AnalyzeTrendAsync(metricType, startDate, endDate);
        
        var predictions = new List<ForecastPoint>();
        var lastDataPoint = trendAnalysis.DataPoints.Last();
        var lastIndex = trendAnalysis.DataPoints.Count - 1;

        // Calculate confidence interval based on R²
        var confidenceLevel = trendAnalysis.RSquared * 100;
        var metrics = await GetMetricsByTypeAndPeriod(metricType, startDate, endDate);
        var stdDev = CalculateStandardDeviation(metrics.Select(m => m.Value).ToList());
        var confidenceInterval = 1.96 * stdDev; // 95% confidence

        for (int i = 1; i <= hoursAhead; i++)
        {
            var predictedValue = lastDataPoint.TrendLineValue + (trendAnalysis.SlopePerHour * i);
            
            predictions.Add(new ForecastPoint
            {
                Timestamp = lastDataPoint.Timestamp.AddHours(i),
                PredictedValue = Math.Round(predictedValue, 2),
                LowerBound = Math.Round(predictedValue - confidenceInterval, 2),
                UpperBound = Math.Round(predictedValue + confidenceInterval, 2)
            });
        }

        var warning = trendAnalysis.RSquared < 0.5 
            ? "Low confidence forecast due to weak trend correlation. Use with caution."
            : string.Empty;

        return new ForecastDto
        {
            MetricType = metricType,
            Predictions = predictions,
            ConfidenceLevel = Math.Round(confidenceLevel, 1),
            ForecastMethod = "Linear Regression",
            Warning = warning
        };
    }

    public async Task<AnomalyClassificationDto> ClassifyAnomalyTypeAsync(int anomalyId)
    {
        var anomaly = await _unitOfWork.Anomalies.GetByIdAsync(anomalyId);
        
        if (anomaly == null)
        {
            throw new KeyNotFoundException($"Anomaly with ID {anomalyId} not found");
        }

        // Get metrics around the anomaly time
        var allMetrics = await _unitOfWork.SystemMetrics.GetAllAsync();
        
        var relevantMetrics = allMetrics
            .Where(m => m.MetricName == anomaly.MetricName)
            .Where(m => m.Timestamp >= anomaly.DetectedAt.AddHours(-24) && 
                       m.Timestamp <= anomaly.DetectedAt.AddHours(24))
            .OrderBy(m => m.Timestamp)
            .ToList();

        if (relevantMetrics.Count() < 10)
        {
            return new AnomalyClassificationDto
            {
                AnomalyId = anomalyId,
                Classification = "Unknown",
                Confidence = 0.5,
                Reasoning = "Insufficient data for classification",
                RequiresImmediateAction = true,
                RecommendedAction = "Gather more data and monitor closely"
            };
        }

        // Check if anomaly is isolated (one-off spike)
        var anomalyIndex = relevantMetrics.FindIndex(m => 
            Math.Abs((m.Timestamp - anomaly.DetectedAt).TotalMinutes) < 5);
        
        if (anomalyIndex == -1) anomalyIndex = relevantMetrics.Count() / 2;

        var beforeValues = relevantMetrics.Take(anomalyIndex).Select(m => m.Value).ToList();
        var afterValues = relevantMetrics.Skip(anomalyIndex + 1).Select(m => m.Value).ToList();

        var beforeMean = beforeValues.Any() ? beforeValues.Average() : 0;
        var afterMean = afterValues.Any() ? afterValues.Average() : 0;
        var anomalyValue = anomaly.DetectedValue;

        // Classify based on pattern
        string classification;
        double confidence;
        string reasoning;
        bool requiresAction;
        string recommendedAction;

        if (Math.Abs(afterMean - beforeMean) < beforeMean * 0.1) // Within 10% of baseline
        {
            classification = "OneOffSpike";
            confidence = 0.85;
            reasoning = "Metric returned to baseline after anomaly. Likely a temporary spike.";
            requiresAction = false;
            recommendedAction = "Document incident for pattern analysis. No immediate action needed.";
        }
        else if (afterMean > beforeMean * 1.2) // Sustained increase
        {
            classification = "SustainedIssue";
            confidence = 0.90;
            reasoning = "Metric remains elevated after anomaly. Indicates a persistent problem.";
            requiresAction = true;
            recommendedAction = "Investigate root cause immediately. Issue is ongoing.";
        }
        else
        {
            classification = "RecurringPattern";
            confidence = 0.75;
            reasoning = "Metric shows pattern that may recur. Requires monitoring.";
            requiresAction = true;
            recommendedAction = "Set up alerts for similar patterns. Investigate underlying cause.";
        }

        return new AnomalyClassificationDto
        {
            AnomalyId = anomalyId,
            Classification = classification,
            Confidence = confidence,
            Reasoning = reasoning,
            RequiresImmediateAction = requiresAction,
            RecommendedAction = recommendedAction
        };
    }

    public async Task<TrendReportDto> GetTrendReportAsync(DateTime startDate, DateTime endDate)
    {
        var metricTypes = new[] { "ResponseTime", "CPUUsage", "MemoryUsage", "ErrorCount", "RequestCount" };
        var metricTrends = new List<MetricTrendSummary>();
        var keyFindings = new List<string>();
        var recommendations = new List<string>();

        foreach (var metricType in metricTypes)
        {
            try
            {
                var trend = await AnalyzeTrendAsync(metricType, startDate, endDate);
                var variance = await AnalyzeVarianceAsync(metricType, startDate, endDate);
                var seasonality = await DetectSeasonalityAsync(metricType, startDate, endDate);

                var changePercentage = trend.SlopePerHour * ((endDate - startDate).TotalHours) / variance.Mean * 100;

                var healthStatus = DetermineHealthStatus(metricType, trend, variance);

                metricTrends.Add(new MetricTrendSummary
                {
                    MetricType = metricType,
                    TrendDirection = trend.TrendDirection,
                    ChangePercentage = Math.Round(changePercentage, 2),
                    HasSeasonality = seasonality.HasSeasonality,
                    StabilityAssessment = variance.StabilityAssessment,
                    HealthStatus = healthStatus
                });

                // Generate key findings
                if (trend.IsSustainedTrend && Math.Abs(changePercentage) > 10)
                {
                    keyFindings.Add($"{metricType}: {trend.TrendDirection} trend of {Math.Abs(changePercentage):F1}% detected");
                }

                if (seasonality.HasSeasonality)
                {
                    keyFindings.Add($"{metricType}: Seasonality detected with {seasonality.PeakPeriods.Count} peak periods");
                }

                // Generate recommendations
                if (healthStatus == "Critical")
                {
                    recommendations.Add($"URGENT: {metricType} requires immediate attention - {trend.Description}");
                }
                else if (healthStatus == "Warning" && trend.TrendDirection == "Upward")
                {
                    recommendations.Add($"Monitor {metricType} closely - trending upward and may become critical");
                }
            }
            catch
            {
                // Skip metrics with insufficient data
                continue;
            }
        }

        // Get anomaly statistics
        var anomalies = await _unitOfWork.Anomalies.GetAllAsync();
        var periodAnomalies = anomalies
            .Where(a => a.DetectedAt >= startDate && a.DetectedAt <= endDate)
            .ToList();

        var totalAnomalies = periodAnomalies.Count;
        var oneOffSpikes = 0;
        var sustainedIssues = 0;

        foreach (var anomaly in periodAnomalies.Take(20)) // Classify up to 20 recent anomalies
        {
            try
            {
                var classification = await ClassifyAnomalyTypeAsync(anomaly.Id);
                if (classification.Classification == "OneOffSpike") oneOffSpikes++;
                else if (classification.Classification == "SustainedIssue") sustainedIssues++;
            }
            catch
            {
                continue;
            }
        }

        return new TrendReportDto
        {
            ReportGeneratedAt = DateTime.UtcNow,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            MetricTrends = metricTrends,
            KeyFindings = keyFindings,
            Recommendations = recommendations,
            TotalAnomaliesDetected = totalAnomalies,
            OneOffSpikes = oneOffSpikes,
            SustainedIssues = sustainedIssues
        };
    }

    #region Helper Methods

    private async Task<List<SystemMetric>> GetMetricsByTypeAndPeriod(string metricType, DateTime startDate, DateTime endDate)
    {
        var allMetrics = await _unitOfWork.SystemMetrics.GetAllAsync();
        
        return allMetrics
            .Where(m => m.MetricName == metricType)
            .Where(m => m.Timestamp >= startDate && m.Timestamp <= endDate)
            .OrderBy(m => m.Timestamp)
            .ToList();
    }

    private double CalculateStandardDeviation(List<double> values)
    {
        if (!values.Any()) return 0;
        
        var mean = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    private (double tStatistic, double pValue) PerformWelchTTest(List<double> sample1, List<double> sample2)
    {
        var mean1 = sample1.Average();
        var mean2 = sample2.Average();
        var var1 = sample1.Sum(x => Math.Pow(x - mean1, 2)) / (sample1.Count - 1);
        var var2 = sample2.Sum(x => Math.Pow(x - mean2, 2)) / (sample2.Count - 1);
        
        var standardError = Math.Sqrt(var1 / sample1.Count + var2 / sample2.Count);
        var tStatistic = (mean1 - mean2) / standardError;
        
        // Simplified p-value approximation (for demonstration)
        // In production, use a proper statistical library
        var pValue = 2 * (1 - NormalCDF(Math.Abs(tStatistic)));
        
        return (tStatistic, Math.Max(0.0001, Math.Min(1.0, pValue)));
    }

    private double NormalCDF(double x)
    {
        // Approximation of cumulative distribution function for standard normal
        return 0.5 * (1 + Erf(x / Math.Sqrt(2)));
    }

    private double Erf(double x)
    {
        // Approximation of error function
        var sign = x >= 0 ? 1 : -1;
        x = Math.Abs(x);
        
        var a1 = 0.254829592;
        var a2 = -0.284496736;
        var a3 = 1.421413741;
        var a4 = -1.453152027;
        var a5 = 1.061405429;
        var p = 0.3275911;
        
        var t = 1.0 / (1.0 + p * x);
        var y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
        
        return sign * y;
    }

    private (double slope, double intercept, double rSquared) CalculateLinearRegression(List<double> xValues, List<double> yValues)
    {
        var n = xValues.Count;
        var sumX = xValues.Sum();
        var sumY = yValues.Sum();
        var sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
        var sumX2 = xValues.Sum(x => x * x);
        
        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;
        
        // Calculate R²
        var yMean = yValues.Average();
        var ssTotal = yValues.Sum(y => Math.Pow(y - yMean, 2));
        var ssResidual = xValues.Zip(yValues, (x, y) => Math.Pow(y - (slope * x + intercept), 2)).Sum();
        var rSquared = 1 - (ssResidual / ssTotal);
        
        return (slope, intercept, rSquared);
    }

    private string DetermineHealthStatus(string metricType, TrendAnalysisDto trend, VarianceAnalysisDto variance)
    {
        // Health status based on metric type and trend
        return metricType switch
        {
            "ResponseTime" => variance.Mean > 300 ? "Critical" : variance.Mean > 200 ? "Warning" : "Healthy",
            "CPUUsage" => variance.Mean > 85 ? "Critical" : variance.Mean > 70 ? "Warning" : "Healthy",
            "MemoryUsage" => variance.Mean > 90 ? "Critical" : variance.Mean > 75 ? "Warning" : "Healthy",
            "ErrorCount" => variance.Mean > 100 ? "Critical" : variance.Mean > 50 ? "Warning" : "Healthy",
            _ => "Healthy"
        };
    }

    #endregion
}
