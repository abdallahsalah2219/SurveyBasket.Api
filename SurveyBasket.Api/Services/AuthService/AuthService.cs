
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

        public async Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            // Find User By Email
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) 
                return null;
            // Check Password 
            var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!isValidPassword) 
                return null;

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
            return new AuthResponse(user.Id,user.Email,user.FirstName,user.LastName,token,expiresIn,refreshToken,refreshTokenExpiration );

        }
        public async Task<AuthResponse?> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            // Get UserId
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return null;

            // Get User By userId
            var user= await _userManager.FindByIdAsync(userId);

            if (user is null)
                return null;
            // Get User Refresh token
            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null) 
                return null;

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
            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, refreshTokenExpiration);

        }

        public async Task<AuthResponse?> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null, CancellationToken cancellationToken = default)
        {
            // Check if user already exists
            if (await _userManager.FindByEmailAsync(email) is not null)
                return null;

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
                return null;

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
            return new AuthResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                token,
                expiresIn,
                refreshToken,
                refreshTokenExpiration
            );
        }
        public async Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return false;

            // Get User By userId
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return false;
            // Get User Refresh token
            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return false;

            //Revoke Refresh Token
            userRefreshToken.RevokedOn = DateTime.UtcNow;

            // Save in Data Base

            await _userManager.UpdateAsync(user);

            return true;


        }
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        
    }
}
