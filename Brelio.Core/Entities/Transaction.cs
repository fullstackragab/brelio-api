namespace Brelio.Core.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string TxSignature { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? SenderAddress { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int Confirmations { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Invoice Invoice { get; set; } = null!;
}
