using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("current")]
    public async Task<ActionResult<CurrentSubscriptionDto>> GetCurrentSubscription()
    {
        var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(GetUserId());
        return Ok(subscription);
    }

    [HttpGet]
    public async Task<ActionResult<List<SubscriptionDto>>> GetSubscriptions()
    {
        var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(GetUserId());
        return Ok(subscriptions);
    }

    [HttpPost]
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

    [HttpPost("upgrade")]
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

    [HttpPost("cancel")]
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

    [HttpGet("can-create-invoice")]
    public async Task<ActionResult<bool>> CanCreateInvoice()
    {
        var canCreate = await _subscriptionService.CanCreateInvoiceAsync(GetUserId());
        return Ok(new { canCreate });
    }

    [HttpGet("usage")]
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
