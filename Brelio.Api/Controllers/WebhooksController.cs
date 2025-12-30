using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Configure webhooks for real-time event notifications
/// </summary>
/// <remarks>
/// Set up webhooks to receive real-time notifications when events occur,
/// such as invoice payments or status changes. Webhooks require Pro plan or higher.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// List all webhooks
    /// </summary>
    /// <remarks>
    /// Returns all configured webhooks for the authenticated user.
    /// </remarks>
    /// <returns>List of webhooks</returns>
    /// <response code="200">Webhooks retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<WebhookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<WebhookDto>>> GetWebhooks()
    {
        var webhooks = await _webhookService.GetUserWebhooksAsync(GetUserId());
        return Ok(webhooks);
    }

    /// <summary>
    /// Create a new webhook
    /// </summary>
    /// <remarks>
    /// Creates a new webhook endpoint. The signing secret is only shown once at creation.
    /// Store it securely to verify webhook payloads.
    /// </remarks>
    /// <param name="request">Webhook configuration including URL and events</param>
    /// <returns>Created webhook with signing secret</returns>
    /// <response code="201">Webhook created successfully</response>
    /// <response code="400">Invalid URL, events, or webhooks not available on plan</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(WebhookWithSecretDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WebhookWithSecretDto>> CreateWebhook([FromBody] CreateWebhookRequest request)
    {
        try
        {
            var result = await _webhookService.CreateWebhookAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetWebhooks), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a webhook
    /// </summary>
    /// <remarks>
    /// Update webhook URL, events, or enabled status.
    /// </remarks>
    /// <param name="id">Webhook ID</param>
    /// <param name="request">Updated webhook configuration</param>
    /// <returns>Updated webhook</returns>
    /// <response code="200">Webhook updated successfully</response>
    /// <response code="400">Invalid URL or events</response>
    /// <response code="404">Webhook not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WebhookDto>> UpdateWebhook(Guid id, [FromBody] UpdateWebhookRequest request)
    {
        try
        {
            var result = await _webhookService.UpdateWebhookAsync(GetUserId(), id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Webhook not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a webhook
    /// </summary>
    /// <remarks>
    /// Permanently deletes a webhook. No further events will be sent to this endpoint.
    /// </remarks>
    /// <param name="id">Webhook ID</param>
    /// <response code="204">Webhook deleted successfully</response>
    /// <response code="404">Webhook not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteWebhook(Guid id)
    {
        try
        {
            await _webhookService.DeleteWebhookAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Webhook not found" });
        }
    }

    /// <summary>
    /// Get webhook delivery history
    /// </summary>
    /// <remarks>
    /// Returns recent delivery attempts for a webhook, including success/failure status.
    /// </remarks>
    /// <param name="id">Webhook ID</param>
    /// <param name="limit">Maximum number of deliveries to return (default: 20)</param>
    /// <returns>List of delivery attempts</returns>
    /// <response code="200">Deliveries retrieved successfully</response>
    /// <response code="404">Webhook not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("{id}/deliveries")]
    [ProducesResponseType(typeof(List<WebhookDeliveryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<WebhookDeliveryDto>>> GetDeliveries(Guid id, [FromQuery] int limit = 20)
    {
        try
        {
            var deliveries = await _webhookService.GetWebhookDeliveriesAsync(GetUserId(), id, limit);
            return Ok(deliveries);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Webhook not found" });
        }
    }

    /// <summary>
    /// Send a test webhook
    /// </summary>
    /// <remarks>
    /// Sends a test payload to the webhook endpoint to verify connectivity.
    /// </remarks>
    /// <param name="id">Webhook ID</param>
    /// <returns>Test result</returns>
    /// <response code="200">Test completed</response>
    /// <response code="404">Webhook not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("{id}/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> TestWebhook(Guid id)
    {
        try
        {
            var success = await _webhookService.TestWebhookAsync(GetUserId(), id);
            return Ok(new { success });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Webhook not found" });
        }
    }

    /// <summary>
    /// Regenerate webhook signing secret
    /// </summary>
    /// <remarks>
    /// Generates a new signing secret for the webhook. The old secret is invalidated immediately.
    /// Update your webhook handler with the new secret.
    /// </remarks>
    /// <param name="id">Webhook ID</param>
    /// <response code="200">Secret regenerated successfully</response>
    /// <response code="404">Webhook not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("{id}/regenerate-secret")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegenerateSecret(Guid id)
    {
        try
        {
            await _webhookService.RegenerateSecretAsync(GetUserId(), id);
            return Ok(new { message = "Secret regenerated" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Webhook not found" });
        }
    }

    /// <summary>
    /// List available webhook events
    /// </summary>
    /// <remarks>
    /// Returns all event types that can be subscribed to via webhooks.
    /// </remarks>
    /// <returns>List of event names</returns>
    /// <response code="200">Events retrieved successfully</response>
    [HttpGet("events")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public ActionResult<List<string>> GetAvailableEvents()
    {
        return Ok(WebhookEvents.All);
    }
}
