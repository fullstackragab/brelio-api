using Brelio.Core.Entities;

namespace Brelio.Core.DTOs;

public record CreateInvoiceRequest(
    string Title,
    decimal Amount,
    int ExpirationHours,
    Guid? WalletId,
    string? ClientName,
    string? ClientEmail,
    string? Notes
);

public record UpdateInvoiceRequest(
    string? Title,
    string? ClientName,
    string? ClientEmail,
    string? Notes
);

public record InvoiceDto(
    Guid Id,
    string ShortCode,
    string Title,
    decimal Amount,
    string Currency,
    InvoiceStatus Status,
    string? ClientName,
    string? ClientEmail,
    string? Notes,
    string WalletAddress,
    DateTime ExpiresAt,
    DateTime? PaidAt,
    DateTime CreatedAt,
    TransactionDto? Transaction
);

public record InvoiceListDto(
    Guid Id,
    string ShortCode,
    string Title,
    decimal Amount,
    string Currency,
    InvoiceStatus Status,
    string? ClientName,
    DateTime ExpiresAt,
    DateTime? PaidAt,
    DateTime CreatedAt
);

public record PublicInvoiceDto(
    string ShortCode,
    string Title,
    decimal Amount,
    string Currency,
    string Status,
    string WalletAddress,
    DateTime ExpiresAt,
    string? ClientName
);

public record TransactionDto(
    string TxSignature,
    decimal Amount,
    string? SenderAddress,
    DateTime? ConfirmedAt
);
