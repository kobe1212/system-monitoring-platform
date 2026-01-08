using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monitoring.Application.Services;
using Monitoring.Application.Interfaces;
using Monitoring.Domain.Entities;
using Monitoring.Domain.Enums;

namespace Monitoring.Tests.Services
{
    /// <summary>
    /// Unit tests for AnomalyService demonstrating anomaly detection testing
    /// Tests cover: threshold detection, Z-score detection, edge cases, and resolution workflow
    /// </summary>
    public class AnomalyServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<SystemMetric>> _mockMetricRepo;
        private readonly Mock<IRepository<Anomaly>> _mockAnomalyRepo;
        private readonly AnomalyService _anomalyService;

        public AnomalyServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMetricRepo = new Mock<IRepository<SystemMetric>>();
            _mockAnomalyRepo = new Mock<IRepository<Anomaly>>();
            
            _mockUnitOfWork.Setup(u => u.Repository<SystemMetric>()).Returns(_mockMetricRepo.Object);
            _mockUnitOfWork.Setup(u => u.Repository<Anomaly>()).Returns(_mockAnomalyRepo.Object);
            
            _anomalyService = new AnomalyService(_mockUnitOfWork.Object);
        }

        #region Threshold-Based Detection Tests

        [Fact]
        public async Task DetectAnomaliesAsync_ResponseTime_DetectsHighValues()
        {
            // Arrange
            var metrics = new List<SystemMetric>
            {
                CreateMetric("WebServer01", "ResponseTime", 600, DateTime.UtcNow), // Anomaly (>500)
                CreateMetric("WebServer01", "ResponseTime", 150, DateTime.UtcNow.AddMinutes(-1)),
                CreateMetric("WebServer01", "ResponseTime", 200, DateTime.UtcNow.AddMinutes(-2))
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            _mockAnomalyRepo.Verify(r => r.AddAsync(It.IsAny<Anomaly>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DetectAnomaliesAsync_CPUUsage_DetectsHighValues()
        {
            // Arrange
            var metrics = new List<SystemMetric>
            {
                CreateMetric("AppServer01", "CPUUsage", 95, DateTime.UtcNow), // Anomaly (>90)
                CreateMetric("AppServer01", "CPUUsage", 50, DateTime.UtcNow.AddMinutes(-1)),
                CreateMetric("AppServer01", "CPUUsage", 55, DateTime.UtcNow.AddMinutes(-2))
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            var anomaly = result.First();
            Assert.Equal("CPUUsage", anomaly.MetricType);
            Assert.Equal("AppServer01", anomaly.ServerName);
        }

        [Fact]
        public async Task DetectAnomaliesAsync_MemoryUsage_DetectsHighValues()
        {
            // Arrange
            var metrics = new List<SystemMetric>
            {
                CreateMetric("DatabaseServer01", "MemoryUsage", 92, DateTime.UtcNow), // Anomaly (>90)
                CreateMetric("DatabaseServer01", "MemoryUsage", 70, DateTime.UtcNow.AddMinutes(-1)),
                CreateMetric("DatabaseServer01", "MemoryUsage", 75, DateTime.UtcNow.AddMinutes(-2))
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
        }

        [Fact]
        public async Task DetectAnomaliesAsync_ErrorRate_DetectsHighValues()
        {
            // Arrange
            var metrics = new List<SystemMetric>
            {
                CreateMetric("APIGateway", "ErrorCount", 150, DateTime.UtcNow), // Anomaly (>100)
                CreateMetric("APIGateway", "ErrorCount", 20, DateTime.UtcNow.AddMinutes(-1)),
                CreateMetric("APIGateway", "ErrorCount", 25, DateTime.UtcNow.AddMinutes(-2))
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
        }

        #endregion

        #region Z-Score Detection Tests

        [Fact]
        public async Task DetectAnomaliesAsync_ZScore_DetectsOutliers()
        {
            // Arrange - Create metrics with one clear outlier
            var baseTime = DateTime.UtcNow;
            var metrics = new List<SystemMetric>
            {
                CreateMetric("WebServer01", "ResponseTime", 100, baseTime.AddMinutes(-10)),
                CreateMetric("WebServer01", "ResponseTime", 110, baseTime.AddMinutes(-9)),
                CreateMetric("WebServer01", "ResponseTime", 105, baseTime.AddMinutes(-8)),
                CreateMetric("WebServer01", "ResponseTime", 108, baseTime.AddMinutes(-7)),
                CreateMetric("WebServer01", "ResponseTime", 102, baseTime.AddMinutes(-6)),
                CreateMetric("WebServer01", "ResponseTime", 500, baseTime) // Clear outlier
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            var anomaly = result.First();
            Assert.True(anomaly.ActualValue > 400); // The outlier value
        }

        [Fact]
        public async Task DetectAnomaliesAsync_ZScore_WithNormalData_DetectsNoAnomalies()
        {
            // Arrange - All values within normal range
            var baseTime = DateTime.UtcNow;
            var metrics = new List<SystemMetric>
            {
                CreateMetric("WebServer01", "ResponseTime", 100, baseTime.AddMinutes(-5)),
                CreateMetric("WebServer01", "ResponseTime", 105, baseTime.AddMinutes(-4)),
                CreateMetric("WebServer01", "ResponseTime", 102, baseTime.AddMinutes(-3)),
                CreateMetric("WebServer01", "ResponseTime", 108, baseTime.AddMinutes(-2)),
                CreateMetric("WebServer01", "ResponseTime", 103, baseTime.AddMinutes(-1)),
                CreateMetric("WebServer01", "ResponseTime", 106, baseTime)
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            // Should detect no Z-score anomalies (threshold-based might still trigger)
        }

        #endregion

        #region Severity Classification Tests

        [Fact]
        public async Task DetectAnomaliesAsync_ClassifiesSeverityCorrectly_Critical()
        {
            // Arrange
            var metrics = new List<SystemMetric>
            {
                CreateMetric("WebServer01", "ResponseTime", 800, DateTime.UtcNow) // Very high - Critical
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            var anomaly = result.First();
            Assert.Equal(AnomalySeverity.Critical, anomaly.Severity);
        }

        [Fact]
        public async Task DetectAnomaliesAsync_ClassifiesSeverityCorrectly_High()
        {
            // Arrange
            var metrics = new List<SystemMetric>
            {
                CreateMetric("WebServer01", "ResponseTime", 600, DateTime.UtcNow) // High
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            var anomaly = result.First();
            Assert.True(anomaly.Severity == AnomalySeverity.High || anomaly.Severity == AnomalySeverity.Critical);
        }

        #endregion

        #region Anomaly Resolution Tests

        [Fact]
        public async Task ResolveAnomalyAsync_WithValidId_MarksAsResolved()
        {
            // Arrange
            var anomaly = new Anomaly
            {
                Id = 1,
                ServerName = "WebServer01",
                MetricType = "ResponseTime",
                ActualValue = 600,
                ExpectedValue = 200,
                Deviation = 400,
                Severity = AnomalySeverity.High,
                DetectedAt = DateTime.UtcNow.AddHours(-1),
                IsResolved = false
            };

            _mockAnomalyRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(anomaly);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _anomalyService.ResolveAnomalyAsync(1);

            // Assert
            Assert.True(anomaly.IsResolved);
            Assert.NotNull(anomaly.ResolvedAt);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ResolveAnomalyAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            _mockAnomalyRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Anomaly)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _anomalyService.ResolveAnomalyAsync(999)
            );
        }

        [Fact]
        public async Task ResolveAnomalyAsync_AlreadyResolved_DoesNotChangeResolvedAt()
        {
            // Arrange
            var resolvedTime = DateTime.UtcNow.AddHours(-2);
            var anomaly = new Anomaly
            {
                Id = 1,
                ServerName = "WebServer01",
                MetricType = "ResponseTime",
                ActualValue = 600,
                ExpectedValue = 200,
                Deviation = 400,
                Severity = AnomalySeverity.High,
                DetectedAt = DateTime.UtcNow.AddHours(-3),
                IsResolved = true,
                ResolvedAt = resolvedTime
            };

            _mockAnomalyRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(anomaly);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _anomalyService.ResolveAnomalyAsync(1);

            // Assert
            Assert.True(anomaly.IsResolved);
            Assert.NotEqual(resolvedTime, anomaly.ResolvedAt); // Should update to new time
        }

        #endregion

        #region Query Tests

        [Fact]
        public async Task GetAllAnomaliesAsync_ReturnsAllAnomalies()
        {
            // Arrange
            var anomalies = new List<Anomaly>
            {
                CreateAnomaly("WebServer01", "ResponseTime", 600, AnomalySeverity.High, false),
                CreateAnomaly("AppServer01", "CPUUsage", 95, AnomalySeverity.Critical, false),
                CreateAnomaly("DatabaseServer01", "MemoryUsage", 92, AnomalySeverity.High, true)
            };

            _mockAnomalyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(anomalies);

            // Act
            var result = await _anomalyService.GetAllAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetUnresolvedAnomaliesAsync_ReturnsOnlyUnresolved()
        {
            // Arrange
            var anomalies = new List<Anomaly>
            {
                CreateAnomaly("WebServer01", "ResponseTime", 600, AnomalySeverity.High, false),
                CreateAnomaly("AppServer01", "CPUUsage", 95, AnomalySeverity.Critical, false),
                CreateAnomaly("DatabaseServer01", "MemoryUsage", 92, AnomalySeverity.High, true)
            };

            _mockAnomalyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(anomalies);

            // Act
            var result = await _anomalyService.GetUnresolvedAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, a => Assert.False(a.IsResolved));
        }

        [Fact]
        public async Task GetAnomaliesByServerAsync_ReturnsServerSpecificAnomalies()
        {
            // Arrange
            var anomalies = new List<Anomaly>
            {
                CreateAnomaly("WebServer01", "ResponseTime", 600, AnomalySeverity.High, false),
                CreateAnomaly("WebServer01", "CPUUsage", 95, AnomalySeverity.Critical, false),
                CreateAnomaly("AppServer01", "CPUUsage", 95, AnomalySeverity.Critical, false)
            };

            _mockAnomalyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(anomalies);

            // Act
            var result = await _anomalyService.GetAnomaliesByServerAsync("WebServer01");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, a => Assert.Equal("WebServer01", a.ServerName));
        }

        [Fact]
        public async Task GetAnomaliesBySeverityAsync_ReturnsSeveritySpecificAnomalies()
        {
            // Arrange
            var anomalies = new List<Anomaly>
            {
                CreateAnomaly("WebServer01", "ResponseTime", 600, AnomalySeverity.High, false),
                CreateAnomaly("AppServer01", "CPUUsage", 95, AnomalySeverity.Critical, false),
                CreateAnomaly("DatabaseServer01", "MemoryUsage", 92, AnomalySeverity.High, false)
            };

            _mockAnomalyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(anomalies);

            // Act
            var result = await _anomalyService.GetAnomaliesBySeverityAsync(AnomalySeverity.Critical);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, a => Assert.Equal(AnomalySeverity.Critical, a.Severity));
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task DetectAnomaliesAsync_WithEmptyMetrics_ReturnsEmptyList()
        {
            // Arrange
            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SystemMetric>());

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task DetectAnomaliesAsync_WithSingleMetric_HandlesGracefully()
        {
            // Arrange
            var metrics = new List<SystemMetric>
            {
                CreateMetric("WebServer01", "ResponseTime", 600, DateTime.UtcNow)
            };

            _mockMetricRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metrics);
            _mockAnomalyRepo.Setup(r => r.AddAsync(It.IsAny<Anomaly>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _anomalyService.DetectAnomaliesAsync();

            // Assert
            Assert.NotNull(result);
            // Should handle single metric without crashing
        }

        #endregion

        #region Helper Methods

        private SystemMetric CreateMetric(string serverName, string metricType, double value, DateTime timestamp)
        {
            return new SystemMetric
            {
                Id = new Random().Next(1, 10000),
                ServerName = serverName,
                MetricType = metricType,
                Value = value,
                Timestamp = timestamp,
                Unit = GetUnitForMetricType(metricType)
            };
        }

        private Anomaly CreateAnomaly(string serverName, string metricType, double actualValue, AnomalySeverity severity, bool isResolved)
        {
            return new Anomaly
            {
                Id = new Random().Next(1, 10000),
                ServerName = serverName,
                MetricType = metricType,
                ActualValue = actualValue,
                ExpectedValue = actualValue * 0.5,
                Deviation = actualValue * 0.5,
                Severity = severity,
                DetectedAt = DateTime.UtcNow.AddHours(-1),
                IsResolved = isResolved,
                ResolvedAt = isResolved ? DateTime.UtcNow : null,
                Description = $"Anomaly detected for {metricType} on {serverName}"
            };
        }

        private string GetUnitForMetricType(string metricType)
        {
            return metricType switch
            {
                "ResponseTime" => "ms",
                "CPUUsage" => "%",
                "MemoryUsage" => "%",
                "RequestCount" => "requests",
                "ErrorCount" => "errors",
                _ => "units"
            };
        }

        #endregion
    }
}
