using Brelio.Core.DTOs;

namespace Brelio.Core.Interfaces;

public interface IWebhookService
{
    Task<List<WebhookDto>> GetUserWebhooksAsync(Guid userId);
    Task<WebhookWithSecretDto> CreateWebhookAsync(Guid userId, CreateWebhookRequest request);
    Task<WebhookDto> UpdateWebhookAsync(Guid userId, Guid webhookId, UpdateWebhookRequest request);
    Task DeleteWebhookAsync(Guid userId, Guid webhookId);
    Task<List<WebhookDeliveryDto>> GetWebhookDeliveriesAsync(Guid userId, Guid webhookId, int limit = 20);
    Task TriggerWebhooksAsync(Guid userId, string eventType, object data);
    Task<bool> TestWebhookAsync(Guid userId, Guid webhookId);
    Task RegenerateSecretAsync(Guid userId, Guid webhookId);
}
