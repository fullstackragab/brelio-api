namespace Brelio.Core.Constants;

/// <summary>
/// Currency configuration for Brelio.
/// All invoices are USDC-only on Solana network.
/// SOL is only accepted for subscription payments.
/// </summary>
public static class CurrencyConstants
{
    /// <summary>
    /// The only supported invoice currency
    /// </summary>
    public const string InvoiceCurrency = "USDC";

    /// <summary>
    /// The only supported network
    /// </summary>
    public const string Network = "Solana";

    /// <summary>
    /// Currencies accepted for subscription payments
    /// </summary>
    public static readonly string[] SubscriptionPaymentCurrencies = ["USDC", "SOL"];

    /// <summary>
    /// USDC token mint address on Solana mainnet
    /// </summary>
    public const string UsdcMintAddress = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v";

    /// <summary>
    /// Validates that the currency is valid for invoices (USDC only)
    /// </summary>
    public static bool IsValidInvoiceCurrency(string currency) =>
        string.Equals(currency, InvoiceCurrency, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Validates that the currency is valid for subscription payments (USDC or SOL)
    /// </summary>
    public static bool IsValidSubscriptionPaymentCurrency(string currency) =>
        SubscriptionPaymentCurrencies.Any(c => string.Equals(c, currency, StringComparison.OrdinalIgnoreCase));
}
