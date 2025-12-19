namespace Brelio.Core.Interfaces;

public interface ISolanaService
{
    Task<List<SolanaTransaction>> GetRecentUsdcTransactionsAsync(string walletAddress);
    bool ValidateAddress(string address);
}

public record SolanaTransaction(
    string Signature,
    decimal Amount,
    string? FromAddress,
    string ToAddress,
    DateTime Timestamp
);
