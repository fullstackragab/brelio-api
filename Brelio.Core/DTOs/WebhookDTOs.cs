namespace Brelio.Core.DTOs;

public record CreateWebhookRequest(
    string Url,
    List<string> Events
);

public record UpdateWebhookRequest(
    string? Url,
    List<string>? Events,
    bool? IsActive
);

public record WebhookDto(
    Guid Id,
    string Url,
    List<string> Events,
    bool IsActive,
    int FailureCount,
    DateTime? LastTriggeredAt,
    DateTime? LastSuccessAt,
    DateTime CreatedAt
);

public record WebhookWithSecretDto(
    Guid Id,
    string Url,
    string Secret,
    List<string> Events,
    bool IsActive,
    DateTime CreatedAt
);

public record WebhookDeliveryDto(
    Guid Id,
    string EventType,
    int StatusCode,
    bool Success,
    int Attempts,
    DateTime CreatedAt,
    DateTime? DeliveredAt
);

public record TestWebhookRequest(
    Guid WebhookId
);

public record WebhookPayload(
    string Event,
    DateTime Timestamp,
    object Data
);
