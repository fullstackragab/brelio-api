namespace Brelio.Core.DTOs;

/// <summary>
/// Dashboard summary statistics
/// </summary>
public record DashboardStatsDto(
    /// <summary>Total USDC received from paid invoices</summary>
    decimal TotalReceived,
    /// <summary>Total number of invoices created</summary>
    int TotalInvoices,
    /// <summary>Number of pending invoices</summary>
    int PendingInvoices,
    /// <summary>Number of paid invoices</summary>
    int PaidInvoices,
    /// <summary>Number of expired invoices</summary>
    int ExpiredInvoices,
    /// <summary>Most recent invoices</summary>
    List<RecentInvoiceDto> RecentInvoices
);

/// <summary>
/// Recent invoice summary for dashboard
/// </summary>
public record RecentInvoiceDto(
    /// <summary>Invoice ID</summary>
    Guid Id,
    /// <summary>Short code for payment URLs</summary>
    string ShortCode,
    /// <summary>Invoice title</summary>
    string Title,
    /// <summary>Amount in USDC</summary>
    decimal Amount,
    /// <summary>Current status</summary>
    string Status,
    /// <summary>When the invoice was created</summary>
    DateTime CreatedAt
);
