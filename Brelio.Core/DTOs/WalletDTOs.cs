namespace Brelio.Core.DTOs;

public record CreateWalletRequest(
    string Address,
    string? Label
);

public record UpdateWalletRequest(
    string? Label,
    bool? IsPrimary
);

public record WalletDto(
    Guid Id,
    string Address,
    string? Label,
    bool IsPrimary,
    DateTime CreatedAt
);
