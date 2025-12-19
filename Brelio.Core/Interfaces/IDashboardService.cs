using Brelio.Core.DTOs;

namespace Brelio.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId);
}
