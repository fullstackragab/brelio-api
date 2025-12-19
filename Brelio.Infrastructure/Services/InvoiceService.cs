using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Brelio.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly BrelioDbContext _context;

    public InvoiceService(BrelioDbContext context)
    {
        _context = context;
    }

    public async Task<List<InvoiceListDto>> GetUserInvoicesAsync(Guid userId, string? status = null)
    {
        var query = _context.Invoices.Where(i => i.UserId == userId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<InvoiceStatus>(status, true, out var statusEnum))
        {
            query = query.Where(i => i.Status == statusEnum);
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvoiceListDto(
                i.Id,
                i.ShortCode,
                i.Title,
                i.Amount,
                i.Currency,
                i.Status,
                i.ClientName,
                i.ExpiresAt,
                i.PaidAt,
                i.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(Guid userId, Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Wallet)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId);

        if (invoice == null) return null;

        var transaction = invoice.Transactions.FirstOrDefault();

        return new InvoiceDto(
            invoice.Id,
            invoice.ShortCode,
            invoice.Title,
            invoice.Amount,
            invoice.Currency,
            invoice.Status,
            invoice.ClientName,
            invoice.ClientEmail,
            invoice.Notes,
            invoice.Wallet.Address,
            invoice.ExpiresAt,
            invoice.PaidAt,
            invoice.CreatedAt,
            transaction != null ? new TransactionDto(
                transaction.TxSignature,
                transaction.Amount,
                transaction.SenderAddress,
                transaction.ConfirmedAt
            ) : null
        );
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(Guid userId, CreateInvoiceRequest request)
    {
        // Get wallet (use provided or primary)
        Wallet? wallet;
        if (request.WalletId.HasValue)
        {
            wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == request.WalletId && w.UserId == userId);
        }
        else
        {
            wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId && w.IsPrimary);
        }

        if (wallet == null)
        {
            throw new InvalidOperationException("No wallet found. Please add a wallet first.");
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WalletId = wallet.Id,
            ShortCode = GenerateShortCode(),
            Title = request.Title,
            Amount = request.Amount,
            Currency = "USDC",
            ClientName = request.ClientName,
            ClientEmail = request.ClientEmail,
            Notes = request.Notes,
            ExpiresAt = DateTime.UtcNow.AddHours(request.ExpirationHours)
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return new InvoiceDto(
            invoice.Id,
            invoice.ShortCode,
            invoice.Title,
            invoice.Amount,
            invoice.Currency,
            invoice.Status,
            invoice.ClientName,
            invoice.ClientEmail,
            invoice.Notes,
            wallet.Address,
            invoice.ExpiresAt,
            invoice.PaidAt,
            invoice.CreatedAt,
            null
        );
    }

    public async Task<InvoiceDto> UpdateInvoiceAsync(Guid userId, Guid invoiceId, UpdateInvoiceRequest request)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Wallet)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId)
            ?? throw new KeyNotFoundException("Invoice not found");

        if (invoice.Status != InvoiceStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invoices can be updated");
        }

        if (request.Title != null) invoice.Title = request.Title;
        if (request.ClientName != null) invoice.ClientName = request.ClientName;
        if (request.ClientEmail != null) invoice.ClientEmail = request.ClientEmail;
        if (request.Notes != null) invoice.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return new InvoiceDto(
            invoice.Id,
            invoice.ShortCode,
            invoice.Title,
            invoice.Amount,
            invoice.Currency,
            invoice.Status,
            invoice.ClientName,
            invoice.ClientEmail,
            invoice.Notes,
            invoice.Wallet.Address,
            invoice.ExpiresAt,
            invoice.PaidAt,
            invoice.CreatedAt,
            null
        );
    }

    public async Task CancelInvoiceAsync(Guid userId, Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId)
            ?? throw new KeyNotFoundException("Invoice not found");

        if (invoice.Status != InvoiceStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invoices can be cancelled");
        }

        invoice.Status = InvoiceStatus.Cancelled;
        await _context.SaveChangesAsync();
    }

    public async Task<PublicInvoiceDto?> GetPublicInvoiceAsync(string shortCode)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Wallet)
            .FirstOrDefaultAsync(i => i.ShortCode == shortCode);

        if (invoice == null) return null;

        // Check if expired
        if (invoice.Status == InvoiceStatus.Pending && invoice.ExpiresAt < DateTime.UtcNow)
        {
            invoice.Status = InvoiceStatus.Expired;
            await _context.SaveChangesAsync();
        }

        return new PublicInvoiceDto(
            invoice.ShortCode,
            invoice.Title,
            invoice.Amount,
            invoice.Currency,
            invoice.Status.ToString(),
            invoice.Wallet.Address,
            invoice.ExpiresAt,
            invoice.ClientName
        );
    }

    private static string GenerateShortCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
