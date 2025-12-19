using Brelio.Core.DTOs;
using Brelio.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brelio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public PayController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("{shortCode}")]
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
