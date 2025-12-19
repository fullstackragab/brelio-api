using Brelio.Core.Entities;

namespace Brelio.Core.DTOs;

public record CreateSubscriptionRequest(
    Guid PlanId,
    string BillingCycle,
    string? PaymentTxSignature,
    string? PaymentWalletAddress
);

public record SubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanName,
    string PlanSlug,
    string Status,
    string BillingCycle,
    decimal Amount,
    string Currency,
    DateTime StartDate,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    DateTime? CancelledAt,
    DateTime? EndedAt,
    DateTime CreatedAt
);

public record CurrentSubscriptionDto(
    Guid? SubscriptionId,
    string PlanName,
    string PlanSlug,
    string Status,
    string BillingCycle,
    decimal Amount,
    string Currency,
    DateTime? CurrentPeriodEnd,
    int InvoiceLimit,
    bool IsUnlimited,
    int InvoicesUsedThisMonth,
    PlanFeaturesDto Features
);

public record UpgradeSubscriptionRequest(
    Guid NewPlanId,
    string BillingCycle,
    string? PaymentTxSignature,
    string? PaymentWalletAddress
);

public record CancelSubscriptionRequest(
    string? Reason
);
