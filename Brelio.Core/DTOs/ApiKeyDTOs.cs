namespace Brelio.Core.DTOs;

/// <summary>
/// Request body for creating an API key
/// </summary>
public record CreateApiKeyRequest(
    /// <summary>Descriptive name for the API key</summary>
    string Name
);

/// <summary>
/// API key details (key value is masked)
/// </summary>
public record ApiKeyDto(
    /// <summary>Unique API key ID</summary>
    Guid Id,
    /// <summary>API key name</summary>
    string Name,
    /// <summary>First characters of the key (for identification)</summary>
    string KeyPrefix,
    /// <summary>Whether the key is active</summary>
    bool IsActive,
    /// <summary>When the key was last used</summary>
    DateTime? LastUsedAt,
    /// <summary>Total number of requests made with this key</summary>
    int RequestCount,
    /// <summary>When the key was created</summary>
    DateTime CreatedAt,
    /// <summary>When the key expires (null if no expiration)</summary>
    DateTime? ExpiresAt
);

/// <summary>
/// API key with full key value (shown only at creation)
/// </summary>
public record ApiKeyCreatedDto(
    /// <summary>Unique API key ID</summary>
    Guid Id,
    /// <summary>API key name</summary>
    string Name,
    /// <summary>Full API key value - store securely, shown only once</summary>
    string ApiKey,
    /// <summary>First characters of the key</summary>
    string KeyPrefix,
    /// <summary>When the key was created</summary>
    DateTime CreatedAt
);

/// <summary>
/// Request body for revoking an API key
/// </summary>
public record RevokeApiKeyRequest(
    /// <summary>ID of the API key to revoke</summary>
    Guid KeyId
);
