namespace Brelio.Core.DTOs;

public record PlanDto(
    Guid Id,
    string Name,
    string Slug,
    decimal PriceMonthly,
    decimal PriceYearly,
    string Currency,
    int InvoiceLimit,
    bool IsUnlimited,
    int SortOrder,
    bool IsCustom,
    string? Description,
    string? TargetAudience,
    PlanFeaturesDto Features,
    string SupportLevel
);

public record PlanFeaturesDto(
    bool Usdc,
    bool Sol,
    bool CustomBranding,
    bool AutoConfirmation,
    bool EmailNotifications,
    bool CsvExport,
    bool MultiWallet,
    bool WebhooksApi,
    bool InvoiceMetadata,
    bool AdvancedAnalytics,
    bool MultiUser,
    bool MultiOrg,
    bool TeamRoles,
    bool AutoReminders,
    bool Sla,
    bool CustomIntegrations,
    bool DedicatedSupport
);

public record PlanSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    decimal PriceMonthly,
    decimal PriceYearly,
    string Currency,
    int InvoiceLimit,
    bool IsUnlimited,
    string? Description,
    string? TargetAudience
);
