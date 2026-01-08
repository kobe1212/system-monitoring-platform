using Microsoft.AspNetCore.Mvc;
using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnomaliesController : ControllerBase
{
    private readonly IAnomalyService _anomalyService;
    private readonly ILogger<AnomaliesController> _logger;

    public AnomaliesController(IAnomalyService anomalyService, ILogger<AnomaliesController> logger)
    {
        _anomalyService = anomalyService ?? throw new ArgumentNullException(nameof(anomalyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnomalyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AnomalyDto>>> GetAllAnomalies()
    {
        _logger.LogInformation("Retrieving all anomalies");

        var anomalies = await _anomalyService.GetAllAnomaliesAsync();

        return Ok(anomalies);
    }

    [HttpGet("unresolved")]
    [ProducesResponseType(typeof(IEnumerable<AnomalyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AnomalyDto>>> GetUnresolvedAnomalies()
    {
        _logger.LogInformation("Retrieving unresolved anomalies");

        var anomalies = await _anomalyService.GetUnresolvedAnomaliesAsync();

        return Ok(anomalies);
    }

    [HttpPost("detect")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> DetectAnomalies()
    {
        _logger.LogInformation("Starting anomaly detection");

        await _anomalyService.DetectAnomaliesAsync();

        return Accepted(new { message = "Anomaly detection completed successfully" });
    }

    [HttpPatch("{id}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveAnomaly(int id)
    {
        _logger.LogInformation("Resolving anomaly with ID: {AnomalyId}", id);

        var result = await _anomalyService.ResolveAnomalyAsync(id);

        if (!result)
        {
            return NotFound(new { message = $"Anomaly with ID {id} not found or already resolved" });
        }

        return Ok(new { message = "Anomaly resolved successfully" });
    }
}
