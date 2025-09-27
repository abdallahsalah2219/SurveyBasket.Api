
using Microsoft.AspNetCore.Identity;
using SurveyBasket.Api.Authentication;
using System.Security.Cryptography;

namespace SurveyBasket.Api.Services.AuthService
{
    public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;

        private readonly int _refreshTokenExpiryDays=14;

        public async Task<Result<AuthResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null, CancellationToken cancellationToken = default)
        {
            // Check if user already exists
            if (await _userManager.FindByEmailAsync(email) is not null)
                return Result.Failure<AuthResponse>(UserErrors.EmailAlreadyExist);

            // Create new user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true // or false if email confirmation flow required
            };

            // Attempt to create the user
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCreateAccount);

            // Generate JWT token and refresh token
            var (token, expiresIn) = _jwtProvider.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            // Save refresh token
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration
            });
            await _userManager.UpdateAsync(user);

            // Return authentication response
            var account = new AuthResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                token,
                expiresIn,
                refreshToken,
                refreshTokenExpiration
            );

            return Result.Success(account);
        }

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            // Find User By Email
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) 
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
            // Check Password 
            var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!isValidPassword) 
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            // Generate JWT Token


            var (token, expiresIn) = _jwtProvider.GenerateToken(user);

            // Generate Refresh Token
            var refreshToken = GenerateRefreshToken();

            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            // Save in Data Base
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration
            });

            await _userManager.UpdateAsync(user);

            //return new AuthResponse
            var result = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);
            return Result.Success(result);

        }
        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            // Get UserId
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

            // Get User By userId
            var user= await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);
            // Get User Refresh token
            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;


            // Generate New JWT Token


            var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);

            // Generate New Refresh Token
            var newRefreshToken = GenerateRefreshToken();

            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            // Save in Data Base
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration
            });

            await _userManager.UpdateAsync(user);
            //return new AuthResponse
            var result = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, refreshTokenExpiration);

            return Result.Success(result);

        }

       
        public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return Result.Failure(UserErrors.InvalidJwtToken);

            // Get User By userId
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result.Failure(UserErrors.InvalidJwtToken);
            // Get User Refresh token
            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return Result.Failure(UserErrors.InvalidRefreshToken);

            //Revoke Refresh Token
            userRefreshToken.RevokedOn = DateTime.UtcNow;

            // Save in Data Base

            await _userManager.UpdateAsync(user);

            return Result.Success();


        }
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        
    }
}
