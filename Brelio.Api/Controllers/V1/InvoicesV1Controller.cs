using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers.V1;

/// <summary>
/// Programmatic invoice management via API key authentication
/// </summary>
/// <remarks>
/// Create and manage digital dollar invoices programmatically.
/// Authenticate requests using your API key in the X-API-Key header.
/// </remarks>
[ApiController]
[Route("api/v1/invoices")]
[Produces("application/json")]
public class InvoicesV1Controller : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly IWalletService _walletService;
    private readonly IWebhookService _webhookService;

    public InvoicesV1Controller(
        IInvoiceService invoiceService,
        IWalletService walletService,
        IWebhookService webhookService)
    {
        _invoiceService = invoiceService;
        _walletService = walletService;
        _webhookService = webhookService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Create a new invoice
    /// </summary>
    /// <remarks>
    /// Creates a new digital dollar invoice payable in USDC.
    /// If no wallet is specified, the invoice will use your primary payment address.
    /// Triggers the `invoice.created` webhook event.
    /// </remarks>
    /// <param name="request">Invoice details</param>
    /// <returns>Created invoice with payment details</returns>
    /// <response code="201">Invoice created successfully</response>
    /// <response code="400">Validation error or no wallet configured</response>
    /// <response code="401">Invalid or missing API key</response>
    /// <response code="404">Specified wallet not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceApiRequest request)
    {
        try
        {
            var userId = GetUserId();

            // If no wallet specified, use primary wallet
            Guid? walletId = null;
            if (!string.IsNullOrEmpty(request.WalletId))
            {
                walletId = Guid.Parse(request.WalletId);
            }

            var createRequest = new CreateInvoiceRequest(
                request.Title,
                request.Amount,
                request.ExpirationHours ?? 24,
                walletId,
                request.ClientName,
                request.ClientEmail,
                request.Notes
            );

            var invoice = await _invoiceService.CreateInvoiceAsync(userId, createRequest);

            // Trigger webhook
            await _webhookService.TriggerWebhooksAsync(userId, "invoice.created", new
            {
                invoiceId = invoice.Id,
                shortCode = invoice.ShortCode,
                amount = invoice.Amount,
                currency = invoice.Currency,
                status = invoice.Status,
                clientEmail = invoice.ClientEmail,
                createdAt = invoice.CreatedAt
            });

            return Created($"/api/v1/invoices/{invoice.Id}", invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    /// <remarks>
    /// Returns full details for a specific invoice, including payment information if paid.
    /// </remarks>
    /// <param name="id">Invoice ID</param>
    /// <returns>Invoice details</returns>
    /// <response code="200">Invoice retrieved successfully</response>
    /// <response code="401">Invalid or missing API key</response>
    /// <response code="404">Invoice not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        try
        {
            var invoice = await _invoiceService.GetInvoiceAsync(GetUserId(), id);
            return Ok(invoice);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Invoice not found" });
        }
    }

    /// <summary>
    /// List all invoices
    /// </summary>
    /// <remarks>
    /// Returns all invoices. Optionally filter by status.
    /// </remarks>
    /// <param name="status">Filter by status: Pending, Paid, Expired, or Cancelled</param>
    /// <returns>List of invoices</returns>
    /// <response code="200">Invoices retrieved successfully</response>
    /// <response code="401">Invalid or missing API key</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<InvoiceListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<InvoiceListItemDto>>> ListInvoices([FromQuery] string? status)
    {
        var invoices = await _invoiceService.GetUserInvoicesAsync(GetUserId(), status);
        return Ok(invoices);
    }

    /// <summary>
    /// Cancel an invoice
    /// </summary>
    /// <remarks>
    /// Cancels a pending invoice. Only pending invoices can be cancelled.
    /// Triggers the `invoice.cancelled` webhook event.
    /// </remarks>
    /// <param name="id">Invoice ID</param>
    /// <response code="204">Invoice cancelled successfully</response>
    /// <response code="400">Invoice cannot be cancelled (not pending)</response>
    /// <response code="401">Invalid or missing API key</response>
    /// <response code="404">Invoice not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvoice(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _invoiceService.CancelInvoiceAsync(userId, id);

            // Trigger webhook
            await _webhookService.TriggerWebhooksAsync(userId, "invoice.cancelled", new
            {
                invoiceId = id,
                cancelledAt = DateTime.UtcNow
            });

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

/// <summary>
/// Request body for creating an invoice via API
/// </summary>
public record CreateInvoiceApiRequest(
    /// <summary>Invoice title or description</summary>
    string Title,
    /// <summary>Amount in USDC</summary>
    decimal Amount,
    /// <summary>Hours until invoice expires (default: 24)</summary>
    int? ExpirationHours,
    /// <summary>Wallet ID for payment address (uses primary if not specified)</summary>
    string? WalletId,
    /// <summary>Client's name for record keeping</summary>
    string? ClientName,
    /// <summary>Client's email for notifications</summary>
    string? ClientEmail,
    /// <summary>Additional notes visible on the invoice</summary>
    string? Notes,
    /// <summary>Custom key-value metadata</summary>
    Dictionary<string, string>? Metadata
);

/// <summary>
/// Invoice list item for API responses
/// </summary>
public record InvoiceListItemDto(
    /// <summary>Unique invoice ID</summary>
    Guid Id,
    /// <summary>Short code for payment URLs</summary>
    string ShortCode,
    /// <summary>Invoice title</summary>
    string Title,
    /// <summary>Amount in USDC</summary>
    decimal Amount,
    /// <summary>Currency code (USDC)</summary>
    string Currency,
    /// <summary>Current status: Pending, Paid, Expired, or Cancelled</summary>
    string Status,
    /// <summary>Client name if provided</summary>
    string? ClientName,
    /// <summary>Client email if provided</summary>
    string? ClientEmail,
    /// <summary>When the invoice expires</summary>
    DateTime ExpiresAt,
    /// <summary>When payment was received (null if unpaid)</summary>
    DateTime? PaidAt,
    /// <summary>When the invoice was created</summary>
    DateTime CreatedAt
);
