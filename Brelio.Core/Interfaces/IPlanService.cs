using Brelio.Core.DTOs;

namespace Brelio.Core.Interfaces;

public interface IPlanService
{
    Task<List<PlanDto>> GetAllPlansAsync();
    Task<PlanDto?> GetPlanByIdAsync(Guid planId);
    Task<PlanDto?> GetPlanBySlugAsync(string slug);
}
