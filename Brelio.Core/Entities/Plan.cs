using Brelio.Core.Constants;

namespace Brelio.Core.Entities;

public class Plan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }
    /// <summary>
    /// Subscription payment currency - always USDC (SOL payments are converted)
    /// </summary>
    public string Currency { get; set; } = CurrencyConstants.InvoiceCurrency;
    public int InvoiceLimit { get; set; }
    public bool IsUnlimited { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCustom { get; set; }
    public string? Description { get; set; }
    public string? TargetAudience { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Features - Note: All invoices are USDC only. FeatureSol refers to SOL subscription payments.
    /// <summary>
    /// USDC invoice payments - always true as Brelio only supports USDC invoices
    /// </summary>
    public bool FeatureUsdc { get; set; } = true;
    /// <summary>
    /// Allow paying subscription with SOL (converted to USDC internally)
    /// </summary>
    public bool FeatureSol { get; set; }
    public bool FeatureCustomBranding { get; set; }
    public bool FeatureAutoConfirmation { get; set; }
    public bool FeatureEmailNotifications { get; set; }
    public bool FeatureCsvExport { get; set; }
    public bool FeatureMultiWallet { get; set; }
    public bool FeatureWebhooksApi { get; set; }
    public bool FeatureInvoiceMetadata { get; set; }
    public bool FeatureAdvancedAnalytics { get; set; }
    public bool FeatureMultiUser { get; set; }
    public bool FeatureMultiOrg { get; set; }
    public bool FeatureTeamRoles { get; set; }
    public bool FeatureAutoReminders { get; set; }
    public bool FeatureSla { get; set; }
    public bool FeatureCustomIntegrations { get; set; }
    public bool FeatureDedicatedSupport { get; set; }
    public string SupportLevel { get; set; } = "community";

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
