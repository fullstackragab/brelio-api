namespace Brelio.Core.DTOs;

public record CreateApiKeyRequest(
    string Name
);

public record ApiKeyDto(
    Guid Id,
    string Name,
    string KeyPrefix,
    bool IsActive,
    DateTime? LastUsedAt,
    int RequestCount,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);

public record ApiKeyCreatedDto(
    Guid Id,
    string Name,
    string ApiKey, // Full key - only shown once
    string KeyPrefix,
    DateTime CreatedAt
);

public record RevokeApiKeyRequest(
    Guid KeyId
);
