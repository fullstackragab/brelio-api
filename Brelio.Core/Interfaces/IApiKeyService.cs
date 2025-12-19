using Brelio.Core.DTOs;
using Brelio.Core.Entities;

namespace Brelio.Core.Interfaces;

public interface IApiKeyService
{
    Task<List<ApiKeyDto>> GetUserApiKeysAsync(Guid userId);
    Task<ApiKeyCreatedDto> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request);
    Task RevokeApiKeyAsync(Guid userId, Guid keyId);
    Task<User?> ValidateApiKeyAsync(string apiKey);
    Task RecordApiKeyUsageAsync(Guid keyId);
}
