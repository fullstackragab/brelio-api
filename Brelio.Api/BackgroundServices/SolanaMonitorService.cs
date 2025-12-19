using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Brelio.Api.BackgroundServices;

public class SolanaMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SolanaMonitorService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(15);

    public SolanaMonitorService(IServiceProvider serviceProvider, ILogger<SolanaMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Solana Monitor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPendingInvoices(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pending invoices");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task CheckPendingInvoices(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrelioDbContext>();
        var solanaService = scope.ServiceProvider.GetRequiredService<ISolanaService>();

        // Get pending invoices that haven't expired
        var pendingInvoices = await context.Invoices
            .Include(i => i.Wallet)
            .Where(i => i.Status == InvoiceStatus.Pending && i.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(stoppingToken);

        if (pendingInvoices.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Checking {Count} pending invoices", pendingInvoices.Count);

        // Group by wallet address to minimize RPC calls
        var invoicesByWallet = pendingInvoices.GroupBy(i => i.Wallet.Address);

        foreach (var group in invoicesByWallet)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var walletAddress = group.Key;
            var walletInvoices = group.ToList();

            try
            {
                var transactions = await solanaService.GetRecentUsdcTransactionsAsync(walletAddress);

                foreach (var invoice in walletInvoices)
                {
                    // Find matching transaction
                    var matchingTx = transactions.FirstOrDefault(tx =>
                        tx.Amount >= invoice.Amount &&
                        tx.Timestamp >= invoice.CreatedAt &&
                        tx.Timestamp <= invoice.ExpiresAt);

                    if (matchingTx != null)
                    {
                        // Check if this transaction was already used for another invoice
                        var txAlreadyUsed = await context.Transactions
                            .AnyAsync(t => t.TxSignature == matchingTx.Signature, stoppingToken);

                        if (!txAlreadyUsed)
                        {
                            _logger.LogInformation(
                                "Payment detected for invoice {InvoiceId}: {Amount} USDC, Tx: {TxSignature}",
                                invoice.Id, matchingTx.Amount, matchingTx.Signature);

                            invoice.Status = InvoiceStatus.Paid;
                            invoice.PaidAt = matchingTx.Timestamp;

                            var transaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                InvoiceId = invoice.Id,
                                TxSignature = matchingTx.Signature,
                                Amount = matchingTx.Amount,
                                SenderAddress = matchingTx.FromAddress,
                                ConfirmedAt = matchingTx.Timestamp,
                                Confirmations = 1
                            };

                            context.Transactions.Add(transaction);
                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check transactions for wallet {Wallet}", walletAddress);
            }

            // Small delay between wallet checks to avoid rate limiting
            await Task.Delay(500, stoppingToken);
        }

        // Mark expired invoices
        var expiredInvoices = await context.Invoices
            .Where(i => i.Status == InvoiceStatus.Pending && i.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(stoppingToken);

        foreach (var invoice in expiredInvoices)
        {
            invoice.Status = InvoiceStatus.Expired;
        }

        if (expiredInvoices.Count > 0)
        {
            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Marked {Count} invoices as expired", expiredInvoices.Count);
        }
    }
}
