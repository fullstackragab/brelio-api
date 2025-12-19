namespace Brelio.Core.DTOs;

public record DashboardStatsDto(
    decimal TotalReceived,
    int TotalInvoices,
    int PendingInvoices,
    int PaidInvoices,
    int ExpiredInvoices,
    List<RecentInvoiceDto> RecentInvoices
);

public record RecentInvoiceDto(
    Guid Id,
    string ShortCode,
    string Title,
    decimal Amount,
    string Status,
    DateTime CreatedAt
);
