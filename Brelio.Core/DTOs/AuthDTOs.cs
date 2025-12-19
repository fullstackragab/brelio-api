namespace Brelio.Core.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string? CompanyName,
    string? Country
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record UserDto(
    Guid Id,
    string Email,
    string? CompanyName,
    string? Country,
    string SubscriptionPlan,
    DateTime CreatedAt
);
