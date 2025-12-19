using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Brelio.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly BrelioDbContext _context;

    public SubscriptionService(BrelioDbContext context)
    {
        _context = context;
    }

    public async Task<CurrentSubscriptionDto> GetCurrentSubscriptionAsync(Guid userId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        var invoicesUsed = await GetInvoicesUsedThisMonthAsync(userId);

        if (subscription == null)
        {
            // Return free tier info
            return new CurrentSubscriptionDto(
                null,
                "Free",
                "free",
                "active",
                "monthly",
                0m,
                "USDC",
                null,
                10, // Free tier limit
                false,
                invoicesUsed,
                new PlanFeaturesDto(
                    true, false, false, false, false, false,
                    false, false, false, false, false, false,
                    false, false, false, false, false
                )
            );
        }

        var plan = subscription.Plan;
        return new CurrentSubscriptionDto(
            subscription.Id,
            plan.Name,
            plan.Slug,
            subscription.Status.ToString().ToLower(),
            subscription.BillingCycle.ToString().ToLower(),
            subscription.Amount,
            subscription.Currency,
            subscription.CurrentPeriodEnd,
            plan.InvoiceLimit,
            plan.IsUnlimited,
            invoicesUsed,
            new PlanFeaturesDto(
                plan.FeatureUsdc,
                plan.FeatureSol,
                plan.FeatureCustomBranding,
                plan.FeatureAutoConfirmation,
                plan.FeatureEmailNotifications,
                plan.FeatureCsvExport,
                plan.FeatureMultiWallet,
                plan.FeatureWebhooksApi,
                plan.FeatureInvoiceMetadata,
                plan.FeatureAdvancedAnalytics,
                plan.FeatureMultiUser,
                plan.FeatureMultiOrg,
                plan.FeatureTeamRoles,
                plan.FeatureAutoReminders,
                plan.FeatureSla,
                plan.FeatureCustomIntegrations,
                plan.FeatureDedicatedSupport
            )
        );
    }

    public async Task<List<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId)
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubscriptionDto(
                s.Id,
                s.PlanId,
                s.Plan.Name,
                s.Plan.Slug,
                s.Status.ToString().ToLower(),
                s.BillingCycle.ToString().ToLower(),
                s.Amount,
                s.Currency,
                s.StartDate,
                s.CurrentPeriodStart,
                s.CurrentPeriodEnd,
                s.CancelledAt,
                s.EndedAt,
                s.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(Guid userId, CreateSubscriptionRequest request)
    {
        var plan = await _context.Plans.FindAsync(request.PlanId)
            ?? throw new KeyNotFoundException("Plan not found");

        if (!plan.IsActive)
        {
            throw new InvalidOperationException("Plan is not available");
        }

        // Check if user already has an active subscription
        var existingSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (existingSubscription != null)
        {
            throw new InvalidOperationException("User already has an active subscription. Use upgrade instead.");
        }

        var billingCycle = Enum.Parse<BillingCycle>(request.BillingCycle, ignoreCase: true);
        var amount = billingCycle == BillingCycle.Yearly ? plan.PriceYearly : plan.PriceMonthly;
        var periodEnd = billingCycle == BillingCycle.Yearly
            ? DateTime.UtcNow.AddYears(1)
            : DateTime.UtcNow.AddMonths(1);

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = plan.Id,
            Status = SubscriptionStatus.Active,
            BillingCycle = billingCycle,
            Amount = amount,
            Currency = plan.Currency,
            StartDate = DateTime.UtcNow,
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = periodEnd,
            PaymentTxSignature = request.PaymentTxSignature,
            PaymentWalletAddress = request.PaymentWalletAddress
        };

        _context.Subscriptions.Add(subscription);

        // Update user's subscription plan field
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.SubscriptionPlan = plan.Slug;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new SubscriptionDto(
            subscription.Id,
            subscription.PlanId,
            plan.Name,
            plan.Slug,
            subscription.Status.ToString().ToLower(),
            subscription.BillingCycle.ToString().ToLower(),
            subscription.Amount,
            subscription.Currency,
            subscription.StartDate,
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            subscription.CancelledAt,
            subscription.EndedAt,
            subscription.CreatedAt
        );
    }

    public async Task<SubscriptionDto> UpgradeSubscriptionAsync(Guid userId, UpgradeSubscriptionRequest request)
    {
        var newPlan = await _context.Plans.FindAsync(request.NewPlanId)
            ?? throw new KeyNotFoundException("Plan not found");

        if (!newPlan.IsActive)
        {
            throw new InvalidOperationException("Plan is not available");
        }

        // Cancel existing subscription
        var existingSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (existingSubscription != null)
        {
            existingSubscription.Status = SubscriptionStatus.Cancelled;
            existingSubscription.CancelledAt = DateTime.UtcNow;
            existingSubscription.EndedAt = DateTime.UtcNow;
            existingSubscription.UpdatedAt = DateTime.UtcNow;
        }

        // Create new subscription
        var billingCycle = Enum.Parse<BillingCycle>(request.BillingCycle, ignoreCase: true);
        var amount = billingCycle == BillingCycle.Yearly ? newPlan.PriceYearly : newPlan.PriceMonthly;
        var periodEnd = billingCycle == BillingCycle.Yearly
            ? DateTime.UtcNow.AddYears(1)
            : DateTime.UtcNow.AddMonths(1);

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = newPlan.Id,
            Status = SubscriptionStatus.Active,
            BillingCycle = billingCycle,
            Amount = amount,
            Currency = newPlan.Currency,
            StartDate = DateTime.UtcNow,
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = periodEnd,
            PaymentTxSignature = request.PaymentTxSignature,
            PaymentWalletAddress = request.PaymentWalletAddress
        };

        _context.Subscriptions.Add(subscription);

        // Update user's subscription plan field
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.SubscriptionPlan = newPlan.Slug;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new SubscriptionDto(
            subscription.Id,
            subscription.PlanId,
            newPlan.Name,
            newPlan.Slug,
            subscription.Status.ToString().ToLower(),
            subscription.BillingCycle.ToString().ToLower(),
            subscription.Amount,
            subscription.Currency,
            subscription.StartDate,
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            subscription.CancelledAt,
            subscription.EndedAt,
            subscription.CreatedAt
        );
    }

    public async Task<SubscriptionDto> CancelSubscriptionAsync(Guid userId, CancelSubscriptionRequest request)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            ?? throw new KeyNotFoundException("No active subscription found");

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;
        // Keep EndedAt as CurrentPeriodEnd - user keeps access until end of period

        // Update user's subscription plan to free
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.SubscriptionPlan = "free";
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new SubscriptionDto(
            subscription.Id,
            subscription.PlanId,
            subscription.Plan.Name,
            subscription.Plan.Slug,
            subscription.Status.ToString().ToLower(),
            subscription.BillingCycle.ToString().ToLower(),
            subscription.Amount,
            subscription.Currency,
            subscription.StartDate,
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            subscription.CancelledAt,
            subscription.EndedAt,
            subscription.CreatedAt
        );
    }

    public async Task<bool> CanCreateInvoiceAsync(Guid userId)
    {
        var currentSubscription = await GetCurrentSubscriptionAsync(userId);

        if (currentSubscription.IsUnlimited)
            return true;

        return currentSubscription.InvoicesUsedThisMonth < currentSubscription.InvoiceLimit;
    }

    public async Task<int> GetInvoicesUsedThisMonthAsync(Guid userId)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return await _context.Invoices
            .CountAsync(i => i.UserId == userId && i.CreatedAt >= startOfMonth);
    }
}
