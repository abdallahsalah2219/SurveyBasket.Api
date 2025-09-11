namespace SurveyBasket.Api.Services.AuthService;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null, CancellationToken cancellationToken = default);
    Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResponse?> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
}

