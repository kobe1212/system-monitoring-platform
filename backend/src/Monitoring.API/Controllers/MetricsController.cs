using Microsoft.AspNetCore.Mvc;
using Monitoring.Application.DTOs;
using Monitoring.Application.Interfaces;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricService _metricService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IMetricService metricService, ILogger<MetricsController> logger)
    {
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SystemMetricDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SystemMetricDto>> CreateMetric([FromBody] CreateMetricDto createMetricDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating metric: {MetricName} from source: {Source}", 
            createMetricDto.MetricName, createMetricDto.Source);

        var metric = await _metricService.CreateMetricAsync(createMetricDto);

        return CreatedAtAction(nameof(GetMetricById), new { id = metric.Id }, metric);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SystemMetricDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SystemMetricDto>>> GetMetrics([FromQuery] MetricQueryDto query)
    {
        _logger.LogInformation("Retrieving metrics with query parameters");

        var metrics = await _metricService.GetMetricsAsync(query);

        return Ok(metrics);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SystemMetricDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemMetricDto>> GetMetricById(int id)
    {
        _logger.LogInformation("Retrieving metric with ID: {MetricId}", id);

        var metric = await _metricService.GetMetricByIdAsync(id);

        if (metric == null)
        {
            return NotFound(new { message = $"Metric with ID {id} not found" });
        }

        return Ok(metric);
    }

    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<SystemMetricDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SystemMetricDto>>> GetRecentMetrics([FromQuery] int count = 100)
    {
        _logger.LogInformation("Retrieving {Count} recent metrics", count);

        var metrics = await _metricService.GetRecentMetricsAsync(count);

        return Ok(metrics);
    }
}
