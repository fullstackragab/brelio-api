using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Brelio.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly BrelioDbContext _context;

    public DashboardService(BrelioDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId)
    {
        var invoices = await _context.Invoices
            .Where(i => i.UserId == userId)
            .ToListAsync();

        var totalReceived = await _context.Transactions
            .Where(t => t.Invoice.UserId == userId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var recentInvoices = invoices
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new RecentInvoiceDto(
                i.Id,
                i.ShortCode,
                i.Title,
                i.Amount,
                i.Status.ToString(),
                i.CreatedAt
            ))
            .ToList();

        return new DashboardStatsDto(
            totalReceived,
            invoices.Count,
            invoices.Count(i => i.Status == InvoiceStatus.Pending),
            invoices.Count(i => i.Status == InvoiceStatus.Paid),
            invoices.Count(i => i.Status == InvoiceStatus.Expired),
            recentInvoices
        );
    }
}
