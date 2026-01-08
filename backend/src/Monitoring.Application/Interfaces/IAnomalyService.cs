using Monitoring.Application.DTOs;

namespace Monitoring.Application.Interfaces;

public interface IAnomalyService
{
    Task<IEnumerable<AnomalyDto>> GetAllAnomaliesAsync();
    Task<IEnumerable<AnomalyDto>> GetUnresolvedAnomaliesAsync();
    Task DetectAnomaliesAsync();
    Task<bool> ResolveAnomalyAsync(int anomalyId);
}
