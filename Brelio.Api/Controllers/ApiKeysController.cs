using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ApiKeyDto>>> GetApiKeys()
    {
        var keys = await _apiKeyService.GetUserApiKeysAsync(GetUserId());
        return Ok(keys);
    }

    [HttpPost]
    public async Task<ActionResult<ApiKeyCreatedDto>> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            var result = await _apiKeyService.CreateApiKeyAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetApiKeys), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        try
        {
            await _apiKeyService.RevokeApiKeyAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "API key not found" });
        }
    }
}
