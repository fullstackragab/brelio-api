using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Subscription plans and pricing information
/// </summary>
/// <remarks>
/// Browse available Brelio subscription plans. Plans determine invoice limits,
/// features, and pricing. All plans are priced in USDC.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    /// <summary>
    /// List all available plans
    /// </summary>
    /// <remarks>
    /// Returns all active subscription plans with pricing and feature details.
    /// Plans are sorted by price tier from lowest to highest.
    /// </remarks>
    /// <returns>List of available plans</returns>
    /// <response code="200">Plans retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PlanDto>>> GetPlans()
    {
        var plans = await _planService.GetAllPlansAsync();
        return Ok(plans);
    }

    /// <summary>
    /// Get plan details
    /// </summary>
    /// <remarks>
    /// Returns full details for a specific plan by ID or slug.
    /// Use slug for user-friendly URLs (e.g., "starter", "pro", "business").
    /// </remarks>
    /// <param name="idOrSlug">Plan ID (GUID) or slug (e.g., "starter")</param>
    /// <returns>Plan details</returns>
    /// <response code="200">Plan retrieved successfully</response>
    /// <response code="404">Plan not found</response>
    [HttpGet("{idOrSlug}")]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
