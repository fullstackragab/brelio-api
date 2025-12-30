using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

/// <summary>
/// Manage digital dollar invoices - create, update, and track payment status
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// List all invoices
    /// </summary>
    /// <remarks>
    /// Returns all invoices for the authenticated user. Optionally filter by status.
    /// </remarks>
    /// <param name="status">Filter by status: Pending, Paid, Expired, or Cancelled</param>
    /// <returns>List of invoices</returns>
    /// <response code="200">Invoices retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<InvoiceListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<InvoiceListDto>>> GetInvoices([FromQuery] string? status = null)
    {
        var invoices = await _invoiceService.GetUserInvoicesAsync(GetUserId(), status);
        return Ok(invoices);
    }

    /// <summary>
    /// Get invoice details
    /// </summary>
    /// <remarks>
    /// Returns full details for a specific invoice, including payment information if paid.
    /// </remarks>
    /// <param name="id">Invoice ID</param>
    /// <returns>Invoice details</returns>
    /// <response code="200">Invoice retrieved successfully</response>
    /// <response code="404">Invoice not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(GetUserId(), id);
        if (invoice == null)
        {
            return NotFound(new { error = "Invoice not found" });
        }
        return Ok(invoice);
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    /// <remarks>
    /// Creates a new digital dollar invoice. The invoice will be payable in USDC to your configured wallet address.
    /// A unique payment link is generated for sharing with your client.
    /// </remarks>
    /// <param name="request">Invoice details</param>
    /// <returns>Created invoice</returns>
    /// <response code="201">Invoice created successfully</response>
    /// <response code="400">Validation error or no wallet configured</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        try
        {
            var invoice = await _invoiceService.CreateInvoiceAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an invoice
    /// </summary>
    /// <remarks>
    /// Updates invoice details. Only pending invoices can be modified.
    /// </remarks>
    /// <param name="id">Invoice ID</param>
    /// <param name="request">Updated invoice details</param>
    /// <returns>Updated invoice</returns>
    /// <response code="200">Invoice updated successfully</response>
    /// <response code="400">Invoice cannot be modified (not pending)</response>
    /// <response code="404">Invoice not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InvoiceDto>> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        try
        {
            var invoice = await _invoiceService.UpdateInvoiceAsync(GetUserId(), id, request);
            return Ok(invoice);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Invoice not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an invoice
    /// </summary>
    /// <remarks>
    /// Cancels a pending invoice. Only pending invoices can be cancelled.
    /// Paid or expired invoices cannot be cancelled.
    /// </remarks>
    /// <param name="id">Invoice ID</param>
    /// <response code="204">Invoice cancelled successfully</response>
    /// <response code="400">Invoice cannot be cancelled (not pending)</response>
    /// <response code="404">Invoice not found</response>
    /// <response code="401">Not authenticated</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelInvoice(Guid id)
    {
        try
        {
            await _invoiceService.CancelInvoiceAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Invoice not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
