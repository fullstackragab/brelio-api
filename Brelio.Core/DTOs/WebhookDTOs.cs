namespace Brelio.Core.DTOs;

/// <summary>
/// Request body for creating a webhook
/// </summary>
public record CreateWebhookRequest(
    /// <summary>HTTPS URL to receive webhook events</summary>
    string Url,
    /// <summary>Event types to subscribe to</summary>
    List<string> Events
);

/// <summary>
/// Request body for updating a webhook
/// </summary>
public record UpdateWebhookRequest(
    /// <summary>Updated webhook URL</summary>
    string? Url,
    /// <summary>Updated event subscriptions</summary>
    List<string>? Events,
    /// <summary>Enable or disable the webhook</summary>
    bool? IsActive
);

/// <summary>
/// Webhook configuration details
/// </summary>
public record WebhookDto(
    /// <summary>Unique webhook ID</summary>
    Guid Id,
    /// <summary>Webhook endpoint URL</summary>
    string Url,
    /// <summary>Subscribed event types</summary>
    List<string> Events,
    /// <summary>Whether the webhook is active</summary>
    bool IsActive,
    /// <summary>Consecutive delivery failures</summary>
    int FailureCount,
    /// <summary>When last event was sent</summary>
    DateTime? LastTriggeredAt,
    /// <summary>When last successful delivery occurred</summary>
    DateTime? LastSuccessAt,
    /// <summary>When the webhook was created</summary>
    DateTime CreatedAt
);

/// <summary>
/// Webhook with signing secret (shown only at creation)
/// </summary>
public record WebhookWithSecretDto(
    /// <summary>Unique webhook ID</summary>
    Guid Id,
    /// <summary>Webhook endpoint URL</summary>
    string Url,
    /// <summary>Signing secret for payload verification - store securely, shown only once</summary>
    string Secret,
    /// <summary>Subscribed event types</summary>
    List<string> Events,
    /// <summary>Whether the webhook is active</summary>
    bool IsActive,
    /// <summary>When the webhook was created</summary>
    DateTime CreatedAt
);

/// <summary>
/// Webhook delivery attempt details
/// </summary>
public record WebhookDeliveryDto(
    /// <summary>Unique delivery ID</summary>
    Guid Id,
    /// <summary>Event type that was delivered</summary>
    string EventType,
    /// <summary>HTTP status code from endpoint</summary>
    int StatusCode,
    /// <summary>Whether delivery was successful</summary>
    bool Success,
    /// <summary>Number of delivery attempts</summary>
    int Attempts,
    /// <summary>When the delivery was initiated</summary>
    DateTime CreatedAt,
    /// <summary>When successful delivery occurred</summary>
    DateTime? DeliveredAt
);

/// <summary>
/// Request body for testing a webhook
/// </summary>
public record TestWebhookRequest(
    /// <summary>ID of the webhook to test</summary>
    Guid WebhookId
);

/// <summary>
/// Webhook event payload structure
/// </summary>
public record WebhookPayload(
    /// <summary>Event type</summary>
    string Event,
    /// <summary>When the event occurred</summary>
    DateTime Timestamp,
    /// <summary>Event-specific data</summary>
    object Data
);
