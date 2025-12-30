using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Manage API keys for programmatic access
/// </summary>
/// <remarks>
/// Create and manage API keys to integrate Brelio with your applications.
/// API access requires Pro plan or higher.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// List all API keys
    /// </summary>
    /// <remarks>
    /// Returns all API keys for the authenticated user. Key values are masked for security.
    /// </remarks>
    /// <returns>List of API keys</returns>
    /// <response code="200">API keys retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiKeyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ApiKeyDto>>> GetApiKeys()
    {
        var keys = await _apiKeyService.GetUserApiKeysAsync(GetUserId());
        return Ok(keys);
    }

    /// <summary>
    /// Create a new API key
    /// </summary>
    /// <remarks>
    /// Creates a new API key. The full key value is only shown once at creation time.
    /// Store it securely as it cannot be retrieved later.
    /// </remarks>
    /// <param name="request">API key details including name and permissions</param>
    /// <returns>Created API key with full key value</returns>
    /// <response code="201">API key created successfully</response>
    /// <response code="400">API access not available on current plan</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyCreatedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Revoke an API key
    /// </summary>
    /// <remarks>
    /// Permanently revokes an API key. This action cannot be undone.
    /// Any requests using this key will be rejected immediately.
    /// </remarks>
    /// <param name="id">API key ID to revoke</param>
    /// <response code="204">API key revoked successfully</response>
    /// <response code="404">API key not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
