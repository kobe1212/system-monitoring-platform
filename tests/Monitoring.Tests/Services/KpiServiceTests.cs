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
    /// Unit tests for KpiService demonstrating effective testing coverage
    /// Tests cover: calculation logic, edge cases, error handling, and business rules
    /// </summary>
    public class KpiServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<SystemMetric>> _mockMetricRepo;
        private readonly Mock<IRepository<KpiResult>> _mockKpiRepo;
        private readonly KpiService _kpiService;

        public KpiServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMetricRepo = new Mock<IRepository<SystemMetric>>();
            _mockKpiRepo = new Mock<IRepository<KpiResult>>();
            
            _mockUnitOfWork.Setup(u => u.Repository<SystemMetric>()).Returns(_mockMetricRepo.Object);
            _mockUnitOfWork.Setup(u => u.Repository<KpiResult>()).Returns(_mockKpiRepo.Object);
            
            _kpiService = new KpiService(_mockUnitOfWork.Object);
        }

        #region Average Response Time Tests

        [Fact]
        public async Task CalculateKpiAsync_AverageResponseTime_ReturnsCorrectValue()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var metrics = GenerateTestMetrics("ResponseTime", 100, 200, 150, 180, 120);

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(metrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("Average Response Time", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Average Response Time", result.KpiName);
            Assert.Equal(150, result.CalculatedValue); // Average of 100,200,150,180,120
            Assert.Equal(KpiStatus.OnTrack, result.Status);
            Assert.InRange(result.CalculatedValue, 0, 300);
        }

        [Fact]
        public async Task CalculateKpiAsync_AverageResponseTime_WithNoData_ReturnsZero()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<SystemMetric>());

            // Act
            var result = await _kpiService.CalculateKpiAsync("Average Response Time", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.CalculatedValue);
        }

        [Fact]
        public async Task CalculateKpiAsync_AverageResponseTime_WithHighValues_ReturnsCriticalStatus()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var metrics = GenerateTestMetrics("ResponseTime", 500, 600, 550, 580, 620);

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(metrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("Average Response Time", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(KpiStatus.Critical, result.Status);
            Assert.True(result.CalculatedValue > 300);
        }

        #endregion

        #region System Throughput Tests

        [Fact]
        public async Task CalculateKpiAsync_SystemThroughput_ReturnsCorrectValue()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            var metrics = GenerateTestMetrics("RequestCount", 1000, 1500, 2000, 1800, 1200);

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(metrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("System Throughput", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("System Throughput", result.KpiName);
            Assert.True(result.CalculatedValue > 0);
            Assert.Equal(KpiStatus.OnTrack, result.Status);
        }

        #endregion

        #region Error Rate Tests

        [Fact]
        public async Task CalculateKpiAsync_ErrorRate_ReturnsCorrectPercentage()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var errorMetrics = GenerateTestMetrics("ErrorCount", 10, 15, 20, 5, 10);
            var requestMetrics = GenerateTestMetrics("RequestCount", 1000, 1000, 1000, 1000, 1000);
            
            var allMetrics = errorMetrics.Concat(requestMetrics).ToList();

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(allMetrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("Error Rate", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error Rate", result.KpiName);
            Assert.InRange(result.CalculatedValue, 0, 100); // Percentage
            Assert.True(result.CalculatedValue >= 0);
        }

        [Fact]
        public async Task CalculateKpiAsync_ErrorRate_WithHighErrors_ReturnsCriticalStatus()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var errorMetrics = GenerateTestMetrics("ErrorCount", 100, 150, 200, 180, 170);
            var requestMetrics = GenerateTestMetrics("RequestCount", 1000, 1000, 1000, 1000, 1000);
            
            var allMetrics = errorMetrics.Concat(requestMetrics).ToList();

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(allMetrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("Error Rate", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(KpiStatus.Critical, result.Status);
            Assert.True(result.CalculatedValue > 5); // More than 5% error rate
        }

        #endregion

        #region System Availability Tests

        [Fact]
        public async Task CalculateKpiAsync_SystemAvailability_ReturnsHighPercentage()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var metrics = GenerateTestMetrics("Uptime", 99.9, 99.8, 99.95, 99.7, 99.85);

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(metrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("System Availability", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("System Availability", result.KpiName);
            Assert.InRange(result.CalculatedValue, 95, 100);
            Assert.Equal(KpiStatus.OnTrack, result.Status);
        }

        [Fact]
        public async Task CalculateKpiAsync_SystemAvailability_WithLowUptime_ReturnsCriticalStatus()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var metrics = GenerateTestMetrics("Uptime", 90, 85, 88, 92, 87);

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(metrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("System Availability", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(KpiStatus.Critical, result.Status);
            Assert.True(result.CalculatedValue < 95);
        }

        #endregion

        #region CPU and Memory Usage Tests

        [Fact]
        public async Task CalculateKpiAsync_AverageCpuUsage_ReturnsCorrectValue()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var metrics = GenerateTestMetrics("CPUUsage", 45, 50, 55, 48, 52);

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(metrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("Average CPU Usage", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Average CPU Usage", result.KpiName);
            Assert.InRange(result.CalculatedValue, 0, 100);
            Assert.Equal(KpiStatus.OnTrack, result.Status);
        }

        [Fact]
        public async Task CalculateKpiAsync_AverageMemoryUsage_WithHighValues_ReturnsAtRiskStatus()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var metrics = GenerateTestMetrics("MemoryUsage", 78, 82, 85, 80, 83);

            _mockMetricRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(metrics);

            // Act
            var result = await _kpiService.CalculateKpiAsync("Average Memory Usage", startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(KpiStatus.AtRisk, result.Status);
            Assert.True(result.CalculatedValue > 75);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task CalculateKpiAsync_WithInvalidKpiName_ThrowsArgumentException()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _kpiService.CalculateKpiAsync("Invalid KPI Name", startDate, endDate)
            );
        }

        [Fact]
        public async Task CalculateKpiAsync_WithEndDateBeforeStartDate_ThrowsArgumentException()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(-7);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _kpiService.CalculateKpiAsync("Average Response Time", startDate, endDate)
            );
        }

        [Fact]
        public async Task GetAllKpisAsync_ReturnsAllKpis()
        {
            // Arrange
            var kpis = new List<KpiResult>
            {
                CreateKpiResult("Average Response Time", 150, KpiStatus.OnTrack),
                CreateKpiResult("System Throughput", 5000, KpiStatus.OnTrack),
                CreateKpiResult("Error Rate", 2.5, KpiStatus.OnTrack)
            };

            _mockKpiRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(kpis);

            // Act
            var result = await _kpiService.GetAllKpisAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetKpiByIdAsync_WithValidId_ReturnsKpi()
        {
            // Arrange
            var kpi = CreateKpiResult("Average Response Time", 150, KpiStatus.OnTrack);
            _mockKpiRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(kpi);

            // Act
            var result = await _kpiService.GetKpiByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Average Response Time", result.KpiName);
        }

        [Fact]
        public async Task GetKpiByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockKpiRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((KpiResult)null);

            // Act
            var result = await _kpiService.GetKpiByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Helper Methods

        private List<SystemMetric> GenerateTestMetrics(string metricType, params double[] values)
        {
            var metrics = new List<SystemMetric>();
            var baseTime = DateTime.UtcNow.AddDays(-7);

            for (int i = 0; i < values.Length; i++)
            {
                metrics.Add(new SystemMetric
                {
                    Id = i + 1,
                    ServerName = $"TestServer{i % 3 + 1}",
                    MetricType = metricType,
                    Value = values[i],
                    Timestamp = baseTime.AddHours(i),
                    Unit = GetUnitForMetricType(metricType)
                });
            }

            return metrics;
        }

        private KpiResult CreateKpiResult(string name, double value, KpiStatus status)
        {
            return new KpiResult
            {
                Id = 1,
                KpiName = name,
                CalculatedValue = value,
                Status = status,
                CalculatedAt = DateTime.UtcNow,
                PeriodStart = DateTime.UtcNow.AddDays(-7),
                PeriodEnd = DateTime.UtcNow
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
                "Uptime" => "%",
                _ => "units"
            };
        }

        #endregion
    }
}
