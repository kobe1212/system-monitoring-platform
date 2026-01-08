using Microsoft.AspNetCore.Mvc;
using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KpisController : ControllerBase
{
    private readonly IKpiService _kpiService;
    private readonly ILogger<KpisController> _logger;

    public KpisController(IKpiService kpiService, ILogger<KpisController> logger)
    {
        _kpiService = kpiService ?? throw new ArgumentNullException(nameof(kpiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<KpiResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KpiResultDto>>> GetAllKpis()
    {
        _logger.LogInformation("Retrieving all KPIs");

        var kpis = await _kpiService.GetAllKpisAsync();

        return Ok(kpis);
    }

    [HttpGet("date-range")]
    [ProducesResponseType(typeof(IEnumerable<KpiResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<KpiResultDto>>> GetKpisByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        _logger.LogInformation("Retrieving KPIs from {StartDate} to {EndDate}", startDate, endDate);

        var kpis = await _kpiService.GetKpisByDateRangeAsync(startDate, endDate);

        return Ok(kpis);
    }

    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateKpis()
    {
        _logger.LogInformation("Starting KPI calculation");

        await _kpiService.CalculateKpisAsync();

        return Accepted(new { message = "KPI calculation completed successfully" });
    }
}
