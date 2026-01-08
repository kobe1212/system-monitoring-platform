using Monitoring.Application.DTOs;

namespace Monitoring.Application.Interfaces;

public interface IKpiService
{
    Task<IEnumerable<KpiResultDto>> GetAllKpisAsync();
    Task<IEnumerable<KpiResultDto>> GetKpisByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task CalculateKpisAsync();
}
