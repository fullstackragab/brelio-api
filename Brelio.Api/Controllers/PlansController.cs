using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlanDto>>> GetPlans()
    {
        var plans = await _planService.GetAllPlansAsync();
        return Ok(plans);
    }

    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<PlanDto>> GetPlan(string idOrSlug)
    {
        PlanDto? plan;

        if (Guid.TryParse(idOrSlug, out var id))
        {
            plan = await _planService.GetPlanByIdAsync(id);
        }
        else
        {
            plan = await _planService.GetPlanBySlugAsync(idOrSlug);
        }

        if (plan == null)
        {
            return NotFound(new { error = "Plan not found" });
        }

        return Ok(plan);
    }
}
