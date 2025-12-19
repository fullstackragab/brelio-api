namespace Brelio.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Country { get; set; }
    public string SubscriptionPlan { get; set; } = "free";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
}
