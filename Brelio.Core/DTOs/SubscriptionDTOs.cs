using Brelio.Core.Entities;

namespace Brelio.Core.DTOs;

/// <summary>
/// Request body for creating a subscription
/// </summary>
public record CreateSubscriptionRequest(
    /// <summary>Plan ID to subscribe to</summary>
    Guid PlanId,
    /// <summary>Billing cycle: monthly or yearly</summary>
    string BillingCycle,
    /// <summary>Payment confirmation signature</summary>
    string? PaymentTxSignature,
    /// <summary>Address used for payment</summary>
    string? PaymentWalletAddress
);

/// <summary>
/// Subscription details
/// </summary>
public record SubscriptionDto(
    /// <summary>Unique subscription ID</summary>
    Guid Id,
    /// <summary>Associated plan ID</summary>
    Guid PlanId,
    /// <summary>Plan name</summary>
    string PlanName,
    /// <summary>Plan slug</summary>
    string PlanSlug,
    /// <summary>Subscription status: Active, Cancelled, Expired</summary>
    string Status,
    /// <summary>Billing cycle: monthly or yearly</summary>
    string BillingCycle,
    /// <summary>Subscription amount in USDC</summary>
    decimal Amount,
    /// <summary>Currency code (USDC)</summary>
    string Currency,
    /// <summary>When the subscription started</summary>
    DateTime StartDate,
    /// <summary>Start of current billing period</summary>
    DateTime CurrentPeriodStart,
    /// <summary>End of current billing period</summary>
    DateTime CurrentPeriodEnd,
    /// <summary>When subscription was cancelled (null if active)</summary>
    DateTime? CancelledAt,
    /// <summary>When subscription ended (null if active)</summary>
    DateTime? EndedAt,
    /// <summary>When the subscription was created</summary>
    DateTime CreatedAt
);

/// <summary>
/// Current subscription with usage details
/// </summary>
public record CurrentSubscriptionDto(
    /// <summary>Subscription ID (null if no subscription)</summary>
    Guid? SubscriptionId,
    /// <summary>Plan name</summary>
    string PlanName,
    /// <summary>Plan slug</summary>
    string PlanSlug,
    /// <summary>Subscription status</summary>
    string Status,
    /// <summary>Billing cycle</summary>
    string BillingCycle,
    /// <summary>Subscription amount in USDC</summary>
    decimal Amount,
    /// <summary>Currency code (USDC)</summary>
    string Currency,
    /// <summary>End of current billing period</summary>
    DateTime? CurrentPeriodEnd,
    /// <summary>Maximum invoices per month</summary>
    int InvoiceLimit,
    /// <summary>Whether invoices are unlimited</summary>
    bool IsUnlimited,
    /// <summary>Invoices created this billing period</summary>
    int InvoicesUsedThisMonth,
    /// <summary>Features available on current plan</summary>
    PlanFeaturesDto Features
);

/// <summary>
/// Request body for upgrading subscription
/// </summary>
public record UpgradeSubscriptionRequest(
    /// <summary>New plan ID to upgrade to</summary>
    Guid NewPlanId,
    /// <summary>Billing cycle for new plan</summary>
    string BillingCycle,
    /// <summary>Payment confirmation signature</summary>
    string? PaymentTxSignature,
    /// <summary>Address used for payment</summary>
    string? PaymentWalletAddress
);

/// <summary>
/// Request body for cancelling subscription
/// </summary>
public record CancelSubscriptionRequest(
    /// <summary>Optional reason for cancellation</summary>
    string? Reason
);
