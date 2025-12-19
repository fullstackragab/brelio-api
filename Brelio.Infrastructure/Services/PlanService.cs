using Brelio.Core.DTOs;
using Brelio.Core.Entities;
using Brelio.Core.Interfaces;
using Brelio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Brelio.Infrastructure.Services;

public class PlanService : IPlanService
{
    private readonly BrelioDbContext _context;

    public PlanService(BrelioDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanDto>> GetAllPlansAsync()
    {
        return await _context.Plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<PlanDto?> GetPlanByIdAsync(Guid planId)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);

        return plan == null ? null : MapToDto(plan);
    }

    public async Task<PlanDto?> GetPlanBySlugAsync(string slug)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        return plan == null ? null : MapToDto(plan);
    }

    private static PlanDto MapToDto(Plan p)
    {
        return new PlanDto(
            p.Id,
            p.Name,
            p.Slug,
            p.PriceMonthly,
            p.PriceYearly,
            p.Currency,
            p.InvoiceLimit,
            p.IsUnlimited,
            p.SortOrder,
            p.IsCustom,
            p.Description,
            p.TargetAudience,
            new PlanFeaturesDto(
                p.FeatureUsdc,
                p.FeatureSol,
                p.FeatureCustomBranding,
                p.FeatureAutoConfirmation,
                p.FeatureEmailNotifications,
                p.FeatureCsvExport,
                p.FeatureMultiWallet,
                p.FeatureWebhooksApi,
                p.FeatureInvoiceMetadata,
                p.FeatureAdvancedAnalytics,
                p.FeatureMultiUser,
                p.FeatureMultiOrg,
                p.FeatureTeamRoles,
                p.FeatureAutoReminders,
                p.FeatureSla,
                p.FeatureCustomIntegrations,
                p.FeatureDedicatedSupport
            ),
            p.SupportLevel
        );
    }
}
