using Brelio.Core.DTOs;

namespace Brelio.Core.Interfaces;

public interface ISubscriptionService
{
    Task<CurrentSubscriptionDto> GetCurrentSubscriptionAsync(Guid userId);
    Task<List<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId);
    Task<SubscriptionDto> CreateSubscriptionAsync(Guid userId, CreateSubscriptionRequest request);
    Task<SubscriptionDto> UpgradeSubscriptionAsync(Guid userId, UpgradeSubscriptionRequest request);
    Task<SubscriptionDto> CancelSubscriptionAsync(Guid userId, CancelSubscriptionRequest request);
    Task<bool> CanCreateInvoiceAsync(Guid userId);
    Task<int> GetInvoicesUsedThisMonthAsync(Guid userId);
}
