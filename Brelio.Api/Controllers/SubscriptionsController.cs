using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Manage subscription plans and billing
/// </summary>
/// <remarks>
/// Subscribe to Brelio plans, upgrade or downgrade, and track usage limits.
/// All plan payments are processed in USDC.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get current subscription
    /// </summary>
    /// <remarks>
    /// Returns the user's active subscription with plan details and usage limits.
    /// </remarks>
    /// <returns>Current subscription details</returns>
    /// <response code="200">Subscription retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrentSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentSubscriptionDto>> GetCurrentSubscription()
    {
        var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(GetUserId());
        return Ok(subscription);
    }

    /// <summary>
    /// List subscription history
    /// </summary>
    /// <remarks>
    /// Returns all subscriptions for the user, including past and cancelled subscriptions.
    /// </remarks>
    /// <returns>List of subscriptions</returns>
    /// <response code="200">Subscriptions retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SubscriptionDto>>> GetSubscriptions()
    {
        var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(GetUserId());
        return Ok(subscriptions);
    }

    /// <summary>
    /// Create a new subscription
    /// </summary>
    /// <remarks>
    /// Subscribe to a Brelio plan. Payment is processed in USDC.
    /// </remarks>
    /// <param name="request">Subscription details including plan and billing cycle</param>
    /// <returns>Created subscription</returns>
    /// <response code="201">Subscription created successfully</response>
    /// <response code="400">User already has active subscription or invalid request</response>
    /// <response code="404">Plan not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SubscriptionDto>> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            var subscription = await _subscriptionService.CreateSubscriptionAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetCurrentSubscription), subscription);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Upgrade subscription
    /// </summary>
    /// <remarks>
    /// Upgrade to a higher-tier plan. Prorated billing applies.
    /// </remarks>
    /// <param name="request">New plan details</param>
    /// <returns>Updated subscription</returns>
    /// <response code="200">Subscription upgraded successfully</response>
    /// <response code="400">Cannot upgrade or invalid request</response>
    /// <response code="404">Plan not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("upgrade")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SubscriptionDto>> UpgradeSubscription([FromBody] UpgradeSubscriptionRequest request)
    {
        try
        {
            var subscription = await _subscriptionService.UpgradeSubscriptionAsync(GetUserId(), request);
            return Ok(subscription);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    /// <remarks>
    /// Cancel the current subscription. Access continues until the end of the billing period.
    /// </remarks>
    /// <param name="request">Cancellation details</param>
    /// <returns>Cancelled subscription</returns>
    /// <response code="200">Subscription cancelled successfully</response>
    /// <response code="404">No active subscription found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SubscriptionDto>> CancelSubscription([FromBody] CancelSubscriptionRequest request)
    {
        try
        {
            var subscription = await _subscriptionService.CancelSubscriptionAsync(GetUserId(), request);
            return Ok(subscription);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check if user can create invoices
    /// </summary>
    /// <remarks>
    /// Verifies the user has an active subscription with remaining invoice quota.
    /// </remarks>
    /// <returns>Whether user can create more invoices</returns>
    /// <response code="200">Check completed</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("can-create-invoice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<bool>> CanCreateInvoice()
    {
        var canCreate = await _subscriptionService.CanCreateInvoiceAsync(GetUserId());
        return Ok(new { canCreate });
    }

    /// <summary>
    /// Get current usage statistics
    /// </summary>
    /// <remarks>
    /// Returns invoice usage for the current billing period against plan limits.
    /// </remarks>
    /// <returns>Usage statistics</returns>
    /// <response code="200">Usage retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("usage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetUsage()
    {
        var invoicesUsed = await _subscriptionService.GetInvoicesUsedThisMonthAsync(GetUserId());
        var currentSubscription = await _subscriptionService.GetCurrentSubscriptionAsync(GetUserId());

        return Ok(new
        {
            invoicesUsed,
            invoiceLimit = currentSubscription.InvoiceLimit,
            isUnlimited = currentSubscription.IsUnlimited
        });
    }
}
