namespace Brelio.Core.DTOs;

/// <summary>
/// Subscription plan details
/// </summary>
public record PlanDto(
    /// <summary>Unique plan ID</summary>
    Guid Id,
    /// <summary>Plan name (e.g., Starter, Pro, Business)</summary>
    string Name,
    /// <summary>URL-friendly identifier</summary>
    string Slug,
    /// <summary>Monthly price in USDC</summary>
    decimal PriceMonthly,
    /// <summary>Yearly price in USDC (typically discounted)</summary>
    decimal PriceYearly,
    /// <summary>Currency code (USDC)</summary>
    string Currency,
    /// <summary>Maximum invoices per month (0 if unlimited)</summary>
    int InvoiceLimit,
    /// <summary>Whether invoices are unlimited</summary>
    bool IsUnlimited,
    /// <summary>Display order</summary>
    int SortOrder,
    /// <summary>Whether pricing requires custom quote</summary>
    bool IsCustom,
    /// <summary>Plan description</summary>
    string? Description,
    /// <summary>Target audience description</summary>
    string? TargetAudience,
    /// <summary>Features included in this plan</summary>
    PlanFeaturesDto Features,
    /// <summary>Support level: community, email, priority, dedicated, account_manager</summary>
    string SupportLevel
);

/// <summary>
/// Plan features configuration
/// </summary>
public record PlanFeaturesDto(
    /// <summary>USDC invoice payments</summary>
    bool Usdc,
    /// <summary>SOL subscription payments</summary>
    bool Sol,
    /// <summary>Custom branding on invoices</summary>
    bool CustomBranding,
    /// <summary>Automatic payment confirmation</summary>
    bool AutoConfirmation,
    /// <summary>Email notifications for payments</summary>
    bool EmailNotifications,
    /// <summary>Export invoices to CSV</summary>
    bool CsvExport,
    /// <summary>Multiple wallet addresses</summary>
    bool MultiWallet,
    /// <summary>Webhooks and API access</summary>
    bool WebhooksApi,
    /// <summary>Custom metadata on invoices</summary>
    bool InvoiceMetadata,
    /// <summary>Advanced analytics dashboard</summary>
    bool AdvancedAnalytics,
    /// <summary>Multiple team members</summary>
    bool MultiUser,
    /// <summary>Multiple organizations</summary>
    bool MultiOrg,
    /// <summary>Team role management</summary>
    bool TeamRoles,
    /// <summary>Automatic payment reminders</summary>
    bool AutoReminders,
    /// <summary>Service level agreement</summary>
    bool Sla,
    /// <summary>Custom integrations</summary>
    bool CustomIntegrations,
    /// <summary>Dedicated support representative</summary>
    bool DedicatedSupport
);

/// <summary>
/// Plan summary for list views
/// </summary>
public record PlanSummaryDto(
    /// <summary>Unique plan ID</summary>
    Guid Id,
    /// <summary>Plan name</summary>
    string Name,
    /// <summary>URL-friendly identifier</summary>
    string Slug,
    /// <summary>Monthly price in USDC</summary>
    decimal PriceMonthly,
    /// <summary>Yearly price in USDC</summary>
    decimal PriceYearly,
    /// <summary>Currency code (USDC)</summary>
    string Currency,
    /// <summary>Maximum invoices per month</summary>
    int InvoiceLimit,
    /// <summary>Whether invoices are unlimited</summary>
    bool IsUnlimited,
    /// <summary>Plan description</summary>
    string? Description,
    /// <summary>Target audience description</summary>
    string? TargetAudience
);
