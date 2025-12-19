using Brelio.Core.DTOs;

namespace Brelio.Core.Interfaces;

public interface IInvoiceService
{
    Task<List<InvoiceListDto>> GetUserInvoicesAsync(Guid userId, string? status = null);
    Task<InvoiceDto?> GetInvoiceAsync(Guid userId, Guid invoiceId);
    Task<InvoiceDto> CreateInvoiceAsync(Guid userId, CreateInvoiceRequest request);
    Task<InvoiceDto> UpdateInvoiceAsync(Guid userId, Guid invoiceId, UpdateInvoiceRequest request);
    Task CancelInvoiceAsync(Guid userId, Guid invoiceId);
    Task<PublicInvoiceDto?> GetPublicInvoiceAsync(string shortCode);
}
