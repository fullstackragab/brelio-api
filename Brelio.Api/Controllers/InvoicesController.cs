using System.Security.Claims;
using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<InvoiceListDto>>> GetInvoices([FromQuery] string? status = null)
    {
        var invoices = await _invoiceService.GetUserInvoicesAsync(GetUserId(), status);
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(GetUserId(), id);
        if (invoice == null)
        {
            return NotFound(new { error = "Invoice not found" });
        }
        return Ok(invoice);
    }

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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
