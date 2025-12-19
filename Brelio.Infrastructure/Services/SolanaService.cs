using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Brelio.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Brelio.Infrastructure.Services;

public partial class SolanaService : ISolanaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SolanaService> _logger;
    private readonly string _usdcMint;

    public SolanaService(HttpClient httpClient, IConfiguration configuration, ILogger<SolanaService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _usdcMint = configuration["Solana:UsdcMint"] ?? "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v";
    }

    public async Task<List<SolanaTransaction>> GetRecentUsdcTransactionsAsync(string walletAddress)
    {
        var transactions = new List<SolanaTransaction>();

        try
        {
            // First, get token accounts for the wallet
            var tokenAccounts = await GetTokenAccountsAsync(walletAddress);

            // Find USDC token account
            var usdcAccount = tokenAccounts.FirstOrDefault(t => t.Mint == _usdcMint);
            if (usdcAccount == null)
            {
                _logger.LogDebug("No USDC token account found for wallet {Wallet}", walletAddress);
                return transactions;
            }

            // Get recent signatures for the token account
            var signatures = await GetSignaturesAsync(usdcAccount.Address);

            foreach (var sig in signatures.Take(20)) // Limit to last 20 transactions
            {
                try
                {
                    var tx = await GetTransactionAsync(sig.Signature);
                    if (tx != null)
                    {
                        transactions.Add(tx);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse transaction {Signature}", sig.Signature);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get USDC transactions for wallet {Wallet}", walletAddress);
        }

        return transactions;
    }

    public bool ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Length < 32 || address.Length > 44)
        {
            return false;
        }
        return SolanaAddressRegex().IsMatch(address);
    }

    private async Task<List<TokenAccountInfo>> GetTokenAccountsAsync(string walletAddress)
    {
        var rpcUrl = _configuration["Solana:RpcUrl"] ?? "https://api.devnet.solana.com";
        var request = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "getTokenAccountsByOwner",
            @params = new object[]
            {
                walletAddress,
                new { programId = "TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA" },
                new { encoding = "jsonParsed" }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(rpcUrl, request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var accounts = new List<TokenAccountInfo>();

        if (json.TryGetProperty("result", out var result) &&
            result.TryGetProperty("value", out var value))
        {
            foreach (var account in value.EnumerateArray())
            {
                try
                {
                    var pubkey = account.GetProperty("pubkey").GetString();
                    var mint = account.GetProperty("account")
                        .GetProperty("data")
                        .GetProperty("parsed")
                        .GetProperty("info")
                        .GetProperty("mint")
                        .GetString();

                    if (pubkey != null && mint != null)
                    {
                        accounts.Add(new TokenAccountInfo(pubkey, mint));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to parse token account");
                }
            }
        }

        return accounts;
    }

    private async Task<List<SignatureInfo>> GetSignaturesAsync(string address)
    {
        var rpcUrl = _configuration["Solana:RpcUrl"] ?? "https://api.devnet.solana.com";
        var request = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "getSignaturesForAddress",
            @params = new object[]
            {
                address,
                new { limit = 20 }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(rpcUrl, request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var signatures = new List<SignatureInfo>();

        if (json.TryGetProperty("result", out var result))
        {
            foreach (var sig in result.EnumerateArray())
            {
                var signature = sig.GetProperty("signature").GetString();
                var blockTime = sig.TryGetProperty("blockTime", out var bt) ? bt.GetInt64() : 0;

                if (signature != null)
                {
                    signatures.Add(new SignatureInfo(signature, blockTime));
                }
            }
        }

        return signatures;
    }

    private async Task<SolanaTransaction?> GetTransactionAsync(string signature)
    {
        var rpcUrl = _configuration["Solana:RpcUrl"] ?? "https://api.devnet.solana.com";
        var request = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "getTransaction",
            @params = new object[]
            {
                signature,
                new { encoding = "jsonParsed", maxSupportedTransactionVersion = 0 }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(rpcUrl, request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!json.TryGetProperty("result", out var result) || result.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        try
        {
            var blockTime = result.TryGetProperty("blockTime", out var bt) ? bt.GetInt64() : 0;
            var timestamp = DateTimeOffset.FromUnixTimeSeconds(blockTime).UtcDateTime;

            // Parse token transfers from inner instructions or main instructions
            var meta = result.GetProperty("meta");
            var preBalances = meta.GetProperty("preTokenBalances");
            var postBalances = meta.GetProperty("postTokenBalances");

            // Find USDC transfers
            foreach (var post in postBalances.EnumerateArray())
            {
                if (post.TryGetProperty("mint", out var mint) && mint.GetString() == _usdcMint)
                {
                    var accountIndex = post.GetProperty("accountIndex").GetInt32();
                    var postAmount = decimal.Parse(post.GetProperty("uiTokenAmount").GetProperty("uiAmountString").GetString() ?? "0");

                    // Find corresponding pre-balance
                    decimal preAmount = 0;
                    foreach (var pre in preBalances.EnumerateArray())
                    {
                        if (pre.GetProperty("accountIndex").GetInt32() == accountIndex)
                        {
                            preAmount = decimal.Parse(pre.GetProperty("uiTokenAmount").GetProperty("uiAmountString").GetString() ?? "0");
                            break;
                        }
                    }

                    var amount = postAmount - preAmount;
                    if (amount > 0) // Incoming transfer
                    {
                        var toAddress = result.GetProperty("transaction")
                            .GetProperty("message")
                            .GetProperty("accountKeys")[accountIndex]
                            .GetProperty("pubkey")
                            .GetString();

                        return new SolanaTransaction(
                            signature,
                            amount,
                            null, // Would need more parsing to get sender
                            toAddress ?? "",
                            timestamp
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse transaction {Signature}", signature);
        }

        return null;
    }

    [GeneratedRegex("^[1-9A-HJ-NP-Za-km-z]{32,44}$")]
    private static partial Regex SolanaAddressRegex();

    private record TokenAccountInfo(string Address, string Mint);
    private record SignatureInfo(string Signature, long BlockTime);
}
