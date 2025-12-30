using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Dashboard analytics and summary statistics
/// </summary>
/// <remarks>
/// Retrieve aggregated statistics about invoices, payments, and account activity.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    /// <remarks>
    /// Returns summary statistics including total invoices, payments received,
    /// pending amounts, and recent activity.
    /// </remarks>
    /// <returns>Dashboard statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync(GetUserId());
        return Ok(stats);
    }
}
