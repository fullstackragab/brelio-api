namespace Brelio.Core.DTOs;

/// <summary>
/// Request body for user registration
/// </summary>
public record RegisterRequest(
    /// <summary>Email address for the account</summary>
    string Email,
    /// <summary>Password (minimum 8 characters)</summary>
    string Password,
    /// <summary>Company or business name</summary>
    string? CompanyName,
    /// <summary>Country of operation</summary>
    string? Country
);

/// <summary>
/// Request body for user login
/// </summary>
public record LoginRequest(
    /// <summary>Account email address</summary>
    string Email,
    /// <summary>Account password</summary>
    string Password
);

/// <summary>
/// Authentication response with tokens and user info
/// </summary>
public record AuthResponse(
    /// <summary>JWT access token for API authentication</summary>
    string AccessToken,
    /// <summary>Refresh token for obtaining new access tokens</summary>
    string RefreshToken,
    /// <summary>When the access token expires</summary>
    DateTime ExpiresAt,
    /// <summary>Authenticated user details</summary>
    UserDto User
);

/// <summary>
/// Request body for token refresh
/// </summary>
public record RefreshTokenRequest(
    /// <summary>Current refresh token</summary>
    string RefreshToken
);

/// <summary>
/// User profile information
/// </summary>
public record UserDto(
    /// <summary>Unique user identifier</summary>
    Guid Id,
    /// <summary>User email address</summary>
    string Email,
    /// <summary>Company or business name</summary>
    string? CompanyName,
    /// <summary>Country of operation</summary>
    string? Country,
    /// <summary>Current subscription plan name</summary>
    string SubscriptionPlan,
    /// <summary>When the account was created</summary>
    DateTime CreatedAt
);
