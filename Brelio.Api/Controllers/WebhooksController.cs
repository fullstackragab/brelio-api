using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<WebhookDto>>> GetWebhooks()
    {
        var webhooks = await _webhookService.GetUserWebhooksAsync(GetUserId());
        return Ok(webhooks);
    }

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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

    [HttpGet("{id}/deliveries")]
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

    [HttpPost("{id}/test")]
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

    [HttpPost("{id}/regenerate-secret")]
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

    [HttpGet("events")]
    public ActionResult<List<string>> GetAvailableEvents()
    {
        return Ok(WebhookEvents.All);
    }
}
