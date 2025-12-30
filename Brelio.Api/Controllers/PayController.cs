using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Public payment endpoints for invoice recipients
/// </summary>
/// <remarks>
/// These endpoints are publicly accessible and do not require authentication.
/// They are used by payment pages to display invoice details to payers.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PayController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public PayController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    /// <summary>
    /// Get public invoice details for payment
    /// </summary>
    /// <remarks>
    /// Returns invoice details needed to complete a digital dollar payment.
    /// This endpoint is public and does not require authentication.
    /// The response includes the payment address and amount in USDC.
    /// </remarks>
    /// <param name="shortCode">Invoice short code from the payment link</param>
    /// <returns>Public invoice details</returns>
    /// <response code="200">Invoice retrieved successfully</response>
    /// <response code="404">Invoice not found or expired</response>
    [HttpGet("{shortCode}")]
    [ProducesResponseType(typeof(PublicInvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicInvoiceDto>> GetInvoice(string shortCode)
    {
        var invoice = await _invoiceService.GetPublicInvoiceAsync(shortCode);
        if (invoice == null)
        {
            return NotFound(new { error = "Invoice not found" });
        }
        return Ok(invoice);
    }
}
