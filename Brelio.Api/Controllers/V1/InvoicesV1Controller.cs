using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers.V1;

[ApiController]
[Route("api/v1/invoices")]
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
    [HttpPost]
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
    [HttpGet("{id}")]
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
    [HttpGet]
    public async Task<ActionResult<List<InvoiceListItemDto>>> ListInvoices([FromQuery] string? status)
    {
        var invoices = await _invoiceService.GetUserInvoicesAsync(GetUserId(), status);
        return Ok(invoices);
    }

    /// <summary>
    /// Cancel an invoice
    /// </summary>
    [HttpDelete("{id}")]
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

public record CreateInvoiceApiRequest(
    string Title,
    decimal Amount,
    int? ExpirationHours,
    string? WalletId,
    string? ClientName,
    string? ClientEmail,
    string? Notes,
    Dictionary<string, string>? Metadata
);

public record InvoiceListItemDto(
    Guid Id,
    string ShortCode,
    string Title,
    decimal Amount,
    string Currency,
    string Status,
    string? ClientName,
    string? ClientEmail,
    DateTime ExpiresAt,
    DateTime? PaidAt,
    DateTime CreatedAt
);
