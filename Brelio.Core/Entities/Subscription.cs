namespace Brelio.Core.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USDC";
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime CurrentPeriodStart { get; set; } = DateTime.UtcNow;
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? PaymentTxSignature { get; set; }
    public string? PaymentWalletAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}

public enum SubscriptionStatus
{
    Active,
    PastDue,
    Cancelled,
    Expired,
    Trialing
}

public enum BillingCycle
{
    Monthly,
    Yearly
}
