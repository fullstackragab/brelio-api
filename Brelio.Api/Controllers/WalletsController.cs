using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<WalletDto>>> GetWallets()
    {
        var wallets = await _walletService.GetUserWalletsAsync(GetUserId());
        return Ok(wallets);
    }

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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

    [HttpGet("validate/{address}")]
    public async Task<ActionResult<bool>> ValidateAddress(string address)
    {
        var isValid = await _walletService.ValidateSolanaAddress(address);
        return Ok(new { isValid });
    }
}
