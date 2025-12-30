using Brelio.Core.Entities;

namespace Brelio.Core.DTOs;

/// <summary>
/// Request body for creating an invoice
/// </summary>
public record CreateInvoiceRequest(
    /// <summary>Invoice title or description</summary>
    string Title,
    /// <summary>Amount in USDC</summary>
    decimal Amount,
    /// <summary>Hours until invoice expires</summary>
    int ExpirationHours,
    /// <summary>Wallet ID for payment address (uses primary if not specified)</summary>
    Guid? WalletId,
    /// <summary>Client's name for record keeping</summary>
    string? ClientName,
    /// <summary>Client's email for notifications</summary>
    string? ClientEmail,
    /// <summary>Additional notes visible on the invoice</summary>
    string? Notes
);

/// <summary>
/// Request body for updating an invoice
/// </summary>
public record UpdateInvoiceRequest(
    /// <summary>Updated invoice title</summary>
    string? Title,
    /// <summary>Updated client name</summary>
    string? ClientName,
    /// <summary>Updated client email</summary>
    string? ClientEmail,
    /// <summary>Updated notes</summary>
    string? Notes
);

/// <summary>
/// Full invoice details
/// </summary>
public record InvoiceDto(
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
    InvoiceStatus Status,
    /// <summary>Client name if provided</summary>
    string? ClientName,
    /// <summary>Client email if provided</summary>
    string? ClientEmail,
    /// <summary>Additional notes</summary>
    string? Notes,
    /// <summary>Payment address for receiving funds</summary>
    string WalletAddress,
    /// <summary>When the invoice expires</summary>
    DateTime ExpiresAt,
    /// <summary>When payment was received (null if unpaid)</summary>
    DateTime? PaidAt,
    /// <summary>When the invoice was created</summary>
    DateTime CreatedAt,
    /// <summary>Payment details if invoice is paid</summary>
    TransactionDto? Transaction
);

/// <summary>
/// Invoice summary for list views
/// </summary>
public record InvoiceListDto(
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
    /// <summary>Current status</summary>
    InvoiceStatus Status,
    /// <summary>Client name if provided</summary>
    string? ClientName,
    /// <summary>When the invoice expires</summary>
    DateTime ExpiresAt,
    /// <summary>When payment was received</summary>
    DateTime? PaidAt,
    /// <summary>When the invoice was created</summary>
    DateTime CreatedAt
);

/// <summary>
/// Public invoice details for payment pages
/// </summary>
public record PublicInvoiceDto(
    /// <summary>Short code for payment URLs</summary>
    string ShortCode,
    /// <summary>Invoice title</summary>
    string Title,
    /// <summary>Amount in USDC</summary>
    decimal Amount,
    /// <summary>Currency code (USDC)</summary>
    string Currency,
    /// <summary>Current status</summary>
    string Status,
    /// <summary>Payment address for sending funds</summary>
    string WalletAddress,
    /// <summary>When the invoice expires</summary>
    DateTime ExpiresAt,
    /// <summary>Client name if provided</summary>
    string? ClientName
);

/// <summary>
/// Payment transaction details
/// </summary>
public record TransactionDto(
    /// <summary>Payment confirmation signature</summary>
    string TxSignature,
    /// <summary>Amount received in USDC</summary>
    decimal Amount,
    /// <summary>Sender's address</summary>
    string? SenderAddress,
    /// <summary>When the payment was confirmed</summary>
    DateTime? ConfirmedAt
);
