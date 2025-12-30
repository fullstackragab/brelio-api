namespace Brelio.Core.DTOs;

/// <summary>
/// Request body for adding a wallet
/// </summary>
public record CreateWalletRequest(
    /// <summary>Payment address for receiving digital dollars</summary>
    string Address,
    /// <summary>Optional label to identify this wallet</summary>
    string? Label
);

/// <summary>
/// Request body for updating a wallet
/// </summary>
public record UpdateWalletRequest(
    /// <summary>Updated wallet label</summary>
    string? Label,
    /// <summary>Set as primary payment address</summary>
    bool? IsPrimary
);

/// <summary>
/// Wallet details
/// </summary>
public record WalletDto(
    /// <summary>Unique wallet ID</summary>
    Guid Id,
    /// <summary>Payment address</summary>
    string Address,
    /// <summary>Wallet label</summary>
    string? Label,
    /// <summary>Whether this is the primary payment address</summary>
    bool IsPrimary,
    /// <summary>When the wallet was added</summary>
    DateTime CreatedAt
);
