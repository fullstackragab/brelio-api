using System.Text.RegularExpressions;
using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Brelio.Infrastructure.Services;

public partial class WalletService : IWalletService
{
    private readonly BrelioDbContext _context;

    public WalletService(BrelioDbContext context)
    {
        _context = context;
    }

    public async Task<List<WalletDto>> GetUserWalletsAsync(Guid userId)
    {
        return await _context.Wallets
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.IsPrimary)
            .ThenByDescending(w => w.CreatedAt)
            .Select(w => new WalletDto(w.Id, w.Address, w.Label, w.IsPrimary, w.CreatedAt))
            .ToListAsync();
    }

    public async Task<WalletDto> CreateWalletAsync(Guid userId, CreateWalletRequest request)
    {
        if (!await ValidateSolanaAddress(request.Address))
        {
            throw new ArgumentException("Invalid payment address");
        }

        if (await _context.Wallets.AnyAsync(w => w.UserId == userId && w.Address == request.Address))
        {
            throw new InvalidOperationException("Wallet address already added");
        }

        var isFirstWallet = !await _context.Wallets.AnyAsync(w => w.UserId == userId);

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Address = request.Address,
            Label = request.Label,
            IsPrimary = isFirstWallet
        };

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        return new WalletDto(wallet.Id, wallet.Address, wallet.Label, wallet.IsPrimary, wallet.CreatedAt);
    }

    public async Task<WalletDto> UpdateWalletAsync(Guid userId, Guid walletId, UpdateWalletRequest request)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Wallet not found");

        if (request.Label != null)
        {
            wallet.Label = request.Label;
        }

        if (request.IsPrimary == true && !wallet.IsPrimary)
        {
            // Unset other primary wallets
            var otherWallets = await _context.Wallets
                .Where(w => w.UserId == userId && w.IsPrimary)
                .ToListAsync();
            foreach (var w in otherWallets)
            {
                w.IsPrimary = false;
            }
            wallet.IsPrimary = true;
        }

        await _context.SaveChangesAsync();

        return new WalletDto(wallet.Id, wallet.Address, wallet.Label, wallet.IsPrimary, wallet.CreatedAt);
    }

    public async Task DeleteWalletAsync(Guid userId, Guid walletId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Wallet not found");

        // Check if wallet has invoices
        if (await _context.Invoices.AnyAsync(i => i.WalletId == walletId))
        {
            throw new InvalidOperationException("Cannot delete wallet with existing invoices");
        }

        _context.Wallets.Remove(wallet);
        await _context.SaveChangesAsync();

        // If deleted wallet was primary, set another as primary
        if (wallet.IsPrimary)
        {
            var newPrimary = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);
            if (newPrimary != null)
            {
                newPrimary.IsPrimary = true;
                await _context.SaveChangesAsync();
            }
        }
    }

    public Task<bool> ValidateSolanaAddress(string address)
    {
        // Solana addresses are Base58 encoded and 32-44 characters
        if (string.IsNullOrWhiteSpace(address) || address.Length < 32 || address.Length > 44)
        {
            return Task.FromResult(false);
        }

        // Base58 character set (no 0, O, I, l)
        return Task.FromResult(SolanaAddressRegex().IsMatch(address));
    }

    [GeneratedRegex("^[1-9A-HJ-NP-Za-km-z]{32,44}$")]
    private static partial Regex SolanaAddressRegex();
}
