using System.Security.Cryptography;
using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Brelio.Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly BrelioDbContext _context;
    private const string KeyPrefix = "sk_live_";

    public ApiKeyService(BrelioDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApiKeyDto>> GetUserApiKeysAsync(Guid userId)
    {
        return await _context.ApiKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyDto(
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.IsActive,
                k.LastUsedAt,
                k.RequestCount,
                k.CreatedAt,
                k.ExpiresAt
            ))
            .ToListAsync();
    }

    public async Task<ApiKeyCreatedDto> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request)
    {
        // Check user plan allows API access
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var allowedPlans = new[] { "pro", "business", "enterprise" };
        if (!allowedPlans.Contains(user.SubscriptionPlan.ToLower()))
        {
            throw new InvalidOperationException("API access requires Pro plan or higher");
        }

        // Generate API key
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var rawKey = Convert.ToBase64String(keyBytes).Replace("+", "").Replace("/", "").Replace("=", "");
        var fullKey = $"{KeyPrefix}{rawKey}";
        var keyHash = BCrypt.Net.BCrypt.HashPassword(fullKey);
        var displayPrefix = $"{KeyPrefix}{rawKey[..8]}...";

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            KeyHash = keyHash,
            KeyPrefix = displayPrefix,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();

        return new ApiKeyCreatedDto(
            apiKey.Id,
            apiKey.Name,
            fullKey, // Only returned once!
            displayPrefix,
            apiKey.CreatedAt
        );
    }

    public async Task RevokeApiKeyAsync(Guid userId, Guid keyId)
    {
        var apiKey = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId)
            ?? throw new KeyNotFoundException("API key not found");

        apiKey.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task<User?> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith(KeyPrefix))
        {
            return null;
        }

        // Get all active keys and verify
        var activeKeys = await _context.ApiKeys
            .Include(k => k.User)
            .Where(k => k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();

        foreach (var key in activeKeys)
        {
            if (BCrypt.Net.BCrypt.Verify(apiKey, key.KeyHash))
            {
                return key.User;
            }
        }

        return null;
    }

    public async Task RecordApiKeyUsageAsync(Guid keyId)
    {
        var apiKey = await _context.ApiKeys.FindAsync(keyId);
        if (apiKey != null)
        {
            apiKey.LastUsedAt = DateTime.UtcNow;
            apiKey.RequestCount++;
            await _context.SaveChangesAsync();
        }
    }
}
