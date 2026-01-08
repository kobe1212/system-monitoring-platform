using Monitoring.Domain.Entities;
using Monitoring.Domain.Enums;

namespace Monitoring.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(MonitoringDbContext context)
    {
        if (context.SystemMetrics.Any())
        {
            return;
        }

        var random = new Random(42);
        var now = DateTime.UtcNow;
        var metrics = new List<SystemMetric>();
        var sources = new[] { "WebServer01", "WebServer02", "AppServer01", "DatabaseServer01", "APIGateway" };

        for (int day = 30; day >= 0; day--)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var timestamp = now.AddDays(-day).AddHours(-hour);

                foreach (var source in sources)
                {
                    var baseResponseTime = 150 + random.Next(-20, 20);
                    var responseTime = day == 0 && hour < 2 ? baseResponseTime + random.Next(200, 400) : baseResponseTime;
                    
                    metrics.Add(new SystemMetric
                    {
                        MetricName = "ResponseTime",
                        Value = responseTime,
                        Unit = "ms",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "production,api"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "RequestCount",
                        Value = 800 + random.Next(-200, 400),
                        Unit = "requests",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "production"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "ErrorCount",
                        Value = random.Next(0, 15),
                        Unit = "errors",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "production"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "CPUUsage",
                        Value = 45 + random.Next(-15, 25),
                        Unit = "percent",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "infrastructure"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "MemoryUsage",
                        Value = 60 + random.Next(-10, 20),
                        Unit = "percent",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "infrastructure"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "DiskIO",
                        Value = 120 + random.Next(-30, 50),
                        Unit = "MB/s",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "infrastructure"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "NetworkThroughput",
                        Value = 250 + random.Next(-50, 100),
                        Unit = "Mbps",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "network"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "ActiveConnections",
                        Value = 150 + random.Next(-50, 100),
                        Unit = "connections",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "network"
                    });

                    metrics.Add(new SystemMetric
                    {
                        MetricName = "Uptime",
                        Value = 99.5 + random.NextDouble() * 0.5,
                        Unit = "percent",
                        Timestamp = timestamp,
                        Source = source,
                        Tags = "availability"
                    });
                }
            }
        }

        await context.SystemMetrics.AddRangeAsync(metrics);

        var kpis = new List<KpiResult>
        {
            new KpiResult
            {
                KpiName = "Average Response Time",
                CalculatedValue = 152.5,
                TargetValue = 200.0,
                Status = KpiStatus.OnTarget,
                CalculatedAt = now,
                PeriodStart = now.AddDays(-1),
                PeriodEnd = now,
                Description = "Average response time across all servers in the last 24 hours"
            },
            new KpiResult
            {
                KpiName = "System Throughput",
                CalculatedValue = 4250,
                TargetValue = 4000.0,
                Status = KpiStatus.AboveTarget,
                CalculatedAt = now,
                PeriodStart = now.AddDays(-1),
                PeriodEnd = now,
                Description = "Total requests processed per hour"
            },
            new KpiResult
            {
                KpiName = "Error Rate",
                CalculatedValue = 0.85,
                TargetValue = 1.0,
                Status = KpiStatus.OnTarget,
                CalculatedAt = now,
                PeriodStart = now.AddDays(-1),
                PeriodEnd = now,
                Description = "Percentage of failed requests"
            },
            new KpiResult
            {
                KpiName = "System Availability",
                CalculatedValue = 99.92,
                TargetValue = 99.9,
                Status = KpiStatus.AboveTarget,
                CalculatedAt = now,
                PeriodStart = now.AddDays(-1),
                PeriodEnd = now,
                Description = "Overall system uptime percentage"
            },
            new KpiResult
            {
                KpiName = "Average CPU Usage",
                CalculatedValue = 48.3,
                TargetValue = 70.0,
                Status = KpiStatus.OnTarget,
                CalculatedAt = now,
                PeriodStart = now.AddDays(-1),
                PeriodEnd = now,
                Description = "Average CPU utilization across all servers"
            },
            new KpiResult
            {
                KpiName = "Average Memory Usage",
                CalculatedValue = 62.7,
                TargetValue = 80.0,
                Status = KpiStatus.OnTarget,
                CalculatedAt = now,
                PeriodStart = now.AddDays(-1),
                PeriodEnd = now,
                Description = "Average memory utilization across all servers"
            }
        };

        await context.KpiResults.AddRangeAsync(kpis);

        var anomalies = new List<Anomaly>
        {
            new Anomaly
            {
                MetricName = "ResponseTime",
                DetectedValue = 550.0,
                ExpectedValue = 150.0,
                Deviation = 266.67,
                Severity = AnomalySeverity.High,
                DetectedAt = now.AddHours(-2),
                IsResolved = false,
                Description = "Unusual spike in response time detected on WebServer01"
            },
            new Anomaly
            {
                MetricName = "ErrorCount",
                DetectedValue = 45.0,
                ExpectedValue = 8.0,
                Deviation = 462.5,
                Severity = AnomalySeverity.Critical,
                DetectedAt = now.AddHours(-1),
                IsResolved = false,
                Description = "Critical increase in error count on APIGateway"
            },
            new Anomaly
            {
                MetricName = "CPUUsage",
                DetectedValue = 92.0,
                ExpectedValue = 48.0,
                Deviation = 91.67,
                Severity = AnomalySeverity.Medium,
                DetectedAt = now.AddHours(-3),
                IsResolved = true,
                ResolvedAt = now.AddHours(-2),
                Description = "CPU usage spike on DatabaseServer01 - resolved after optimization"
            }
        };

        await context.Anomalies.AddRangeAsync(anomalies);
        await context.SaveChangesAsync();
    }
}
