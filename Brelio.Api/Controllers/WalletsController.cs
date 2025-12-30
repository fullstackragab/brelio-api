using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Manage payment addresses for receiving digital dollars
/// </summary>
/// <remarks>
/// Configure wallet addresses where you'll receive USDC payments from invoices.
/// Each user can have multiple wallets, with one designated as primary.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// List all wallets
    /// </summary>
    /// <remarks>
    /// Returns all payment addresses configured for the authenticated user.
    /// </remarks>
    /// <returns>List of wallets</returns>
    /// <response code="200">Wallets retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<WalletDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<WalletDto>>> GetWallets()
    {
        var wallets = await _walletService.GetUserWalletsAsync(GetUserId());
        return Ok(wallets);
    }

    /// <summary>
    /// Add a new wallet
    /// </summary>
    /// <remarks>
    /// Adds a new payment address for receiving digital dollars.
    /// The first wallet added automatically becomes the primary wallet.
    /// </remarks>
    /// <param name="request">Wallet details including address and optional label</param>
    /// <returns>Created wallet</returns>
    /// <response code="201">Wallet created successfully</response>
    /// <response code="400">Invalid address or address already exists</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WalletDto>> CreateWallet([FromBody] CreateWalletRequest request)
    {
        try
        {
            var wallet = await _walletService.CreateWalletAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetWallets), wallet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a wallet
    /// </summary>
    /// <remarks>
    /// Update wallet label or set as primary payment address.
    /// </remarks>
    /// <param name="id">Wallet ID</param>
    /// <param name="request">Updated wallet details</param>
    /// <returns>Updated wallet</returns>
    /// <response code="200">Wallet updated successfully</response>
    /// <response code="404">Wallet not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WalletDto>> UpdateWallet(Guid id, [FromBody] UpdateWalletRequest request)
    {
        try
        {
            var wallet = await _walletService.UpdateWalletAsync(GetUserId(), id, request);
            return Ok(wallet);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Wallet not found" });
        }
    }

    /// <summary>
    /// Delete a wallet
    /// </summary>
    /// <remarks>
    /// Removes a payment address. Wallets with existing invoices cannot be deleted.
    /// </remarks>
    /// <param name="id">Wallet ID</param>
    /// <response code="204">Wallet deleted successfully</response>
    /// <response code="400">Wallet has existing invoices</response>
    /// <response code="404">Wallet not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteWallet(Guid id)
    {
        try
        {
            await _walletService.DeleteWalletAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Wallet not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validate a payment address
    /// </summary>
    /// <remarks>
    /// Checks if an address is valid for receiving digital dollar payments.
    /// </remarks>
    /// <param name="address">Payment address to validate</param>
    /// <returns>Validation result</returns>
    /// <response code="200">Validation completed</response>
    [HttpGet("validate/{address}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ValidateAddress(string address)
    {
        var isValid = await _walletService.ValidateSolanaAddress(address);
        return Ok(new { isValid });
    }
}
