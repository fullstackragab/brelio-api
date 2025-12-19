using Brelio.Core.Constants;

namespace Brelio.Core.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    /// <summary>
    /// Invoice currency - always USDC. Brelio only supports USDC invoices on Solana.
    /// </summary>
    public string Currency { get; set; } = CurrencyConstants.InvoiceCurrency;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }
    public string? Notes { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Wallet Wallet { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum InvoiceStatus
{
    Pending,
    Paid,
    Expired,
    Cancelled
}
