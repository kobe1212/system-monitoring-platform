using Microsoft.AspNetCore.Mvc;
using Monitoring.Application.Interfaces;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetDashboardAnalytics([FromQuery] int hours = 24)
    {
        var analytics = await _dashboardService.GetDashboardAnalyticsAsync(hours);
        return Ok(analytics);
    }

    [HttpGet("metrics/{metricName}/trend")]
    public async Task<IActionResult> GetMetricTrend(string metricName, [FromQuery] int hours = 24)
    {
        var trend = await _dashboardService.GetMetricTrendAsync(metricName, hours);
        return Ok(trend);
    }

    [HttpGet("servers/health")]
    public async Task<IActionResult> GetServerHealth()
    {
        var health = await _dashboardService.GetServerHealthAsync();
        return Ok(health);
    }
}
