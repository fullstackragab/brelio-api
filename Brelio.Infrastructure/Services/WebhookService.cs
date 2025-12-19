using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brelio.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly BrelioDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(BrelioDbContext context, HttpClient httpClient, ILogger<WebhookService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<WebhookDto>> GetUserWebhooksAsync(Guid userId)
    {
        return await _context.Webhooks
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WebhookDto(
                w.Id,
                w.Url,
                JsonSerializer.Deserialize<List<string>>(w.Events) ?? new List<string>(),
                w.IsActive,
                w.FailureCount,
                w.LastTriggeredAt,
                w.LastSuccessAt,
                w.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<WebhookWithSecretDto> CreateWebhookAsync(Guid userId, CreateWebhookRequest request)
    {
        // Check user plan allows webhooks
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var allowedPlans = new[] { "pro", "business", "enterprise" };
        if (!allowedPlans.Contains(user.SubscriptionPlan.ToLower()))
        {
            throw new InvalidOperationException("Webhooks require Pro plan or higher");
        }

        // Validate events
        var validEvents = request.Events.Where(e => WebhookEvents.All.Contains(e)).ToList();
        if (validEvents.Count == 0)
        {
            throw new ArgumentException("At least one valid event is required");
        }

        // Validate URL
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http"))
        {
            throw new ArgumentException("Invalid webhook URL");
        }

        var secret = GenerateSecret();

        var webhook = new Webhook
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Url = request.Url,
            Secret = secret,
            Events = JsonSerializer.Serialize(validEvents),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Webhooks.Add(webhook);
        await _context.SaveChangesAsync();

        return new WebhookWithSecretDto(
            webhook.Id,
            webhook.Url,
            webhook.Secret,
            validEvents,
            webhook.IsActive,
            webhook.CreatedAt
        );
    }

    public async Task<WebhookDto> UpdateWebhookAsync(Guid userId, Guid webhookId, UpdateWebhookRequest request)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == webhookId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Webhook not found");

        if (request.Url != null)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "https" && uri.Scheme != "http"))
            {
                throw new ArgumentException("Invalid webhook URL");
            }
            webhook.Url = request.Url;
        }

        if (request.Events != null)
        {
            var validEvents = request.Events.Where(e => WebhookEvents.All.Contains(e)).ToList();
            if (validEvents.Count == 0)
            {
                throw new ArgumentException("At least one valid event is required");
            }
            webhook.Events = JsonSerializer.Serialize(validEvents);
        }

        if (request.IsActive.HasValue)
        {
            webhook.IsActive = request.IsActive.Value;
            if (request.IsActive.Value)
            {
                webhook.FailureCount = 0; // Reset failure count when re-enabling
            }
        }

        webhook.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new WebhookDto(
            webhook.Id,
            webhook.Url,
            JsonSerializer.Deserialize<List<string>>(webhook.Events) ?? new List<string>(),
            webhook.IsActive,
            webhook.FailureCount,
            webhook.LastTriggeredAt,
            webhook.LastSuccessAt,
            webhook.CreatedAt
        );
    }

    public async Task DeleteWebhookAsync(Guid userId, Guid webhookId)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == webhookId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Webhook not found");

        _context.Webhooks.Remove(webhook);
        await _context.SaveChangesAsync();
    }

    public async Task<List<WebhookDeliveryDto>> GetWebhookDeliveriesAsync(Guid userId, Guid webhookId, int limit = 20)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == webhookId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Webhook not found");

        return await _context.WebhookDeliveries
            .Where(d => d.WebhookId == webhookId)
            .OrderByDescending(d => d.CreatedAt)
            .Take(limit)
            .Select(d => new WebhookDeliveryDto(
                d.Id,
                d.EventType,
                d.StatusCode,
                d.Success,
                d.Attempts,
                d.CreatedAt,
                d.DeliveredAt
            ))
            .ToListAsync();
    }

    public async Task TriggerWebhooksAsync(Guid userId, string eventType, object data)
    {
        var webhooks = await _context.Webhooks
            .Where(w => w.UserId == userId && w.IsActive && w.FailureCount < 10)
            .ToListAsync();

        foreach (var webhook in webhooks)
        {
            var events = JsonSerializer.Deserialize<List<string>>(webhook.Events) ?? new List<string>();
            if (!events.Contains(eventType))
            {
                continue;
            }

            var payload = new WebhookPayload(eventType, DateTime.UtcNow, data);
            await SendWebhookAsync(webhook, eventType, payload);
        }
    }

    public async Task<bool> TestWebhookAsync(Guid userId, Guid webhookId)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == webhookId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Webhook not found");

        var testPayload = new WebhookPayload(
            "test",
            DateTime.UtcNow,
            new { message = "This is a test webhook from Brelio" }
        );

        return await SendWebhookAsync(webhook, "test", testPayload, isTest: true);
    }

    public async Task RegenerateSecretAsync(Guid userId, Guid webhookId)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == webhookId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Webhook not found");

        webhook.Secret = GenerateSecret();
        webhook.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task<bool> SendWebhookAsync(Webhook webhook, string eventType, WebhookPayload payload, bool isTest = false)
    {
        var delivery = new WebhookDelivery
        {
            Id = Guid.NewGuid(),
            WebhookId = webhook.Id,
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload),
            Attempts = 1,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var signature = ComputeSignature(jsonPayload, webhook.Secret);

            var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
            request.Headers.Add("X-Brelio-Signature", signature);
            request.Headers.Add("X-Brelio-Event", eventType);
            request.Headers.Add("X-Brelio-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await _httpClient.SendAsync(request, cts.Token);

            delivery.StatusCode = (int)response.StatusCode;
            delivery.Success = response.IsSuccessStatusCode;
            delivery.DeliveredAt = DateTime.UtcNow;

            if (response.IsSuccessStatusCode)
            {
                webhook.LastSuccessAt = DateTime.UtcNow;
                webhook.FailureCount = 0;
            }
            else
            {
                delivery.ResponseBody = await response.Content.ReadAsStringAsync();
                if (!isTest)
                {
                    webhook.FailureCount++;
                }
            }

            webhook.LastTriggeredAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver webhook to {Url}", webhook.Url);
            delivery.StatusCode = 0;
            delivery.Success = false;
            delivery.ResponseBody = ex.Message;
            if (!isTest)
            {
                webhook.FailureCount++;
            }
        }

        if (!isTest)
        {
            _context.WebhookDeliveries.Add(delivery);
        }
        await _context.SaveChangesAsync();

        return delivery.Success;
    }

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLower();
    }

    private static string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLower();
    }
}
