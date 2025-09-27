namespace SurveyBasket.Api.Services.AuthService;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
}

