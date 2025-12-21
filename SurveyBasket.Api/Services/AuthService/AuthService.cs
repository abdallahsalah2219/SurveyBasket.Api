using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace SurveyBasket.Api.Services.AuthService
{
    public class AuthService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager ,
        IJwtProvider jwtProvider,
        ILogger<AuthService> logger ,
        IEmailSender emailSender,
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext context) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ApplicationDbContext _context = context;


        private readonly int _refreshTokenExpiryDays=14;

        public async Task<Result>RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            //Check if Email is exists
            var emailIsExists = await _userManager.Users.AnyAsync(x =>x.Email ==request.Email);
            if (emailIsExists) 
                return Result.Failure(UserErrors.EmailAlreadyExist);
            // Adapt Request to Application User
            var user = request.Adapt<ApplicationUser>();

            //Create User
            var result = await _userManager.CreateAsync(user, request.Password);

            // Check Result
            if (result.Succeeded)
            {
                // Generate a secure email confirmation token for the user
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Encode the token using Base64 URL-safe format to make it valid inside a URL
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                _logger.LogInformation("Email confirmation code:{Code}",  code);



                // Send Email Confirmation
                await SendConfirmationEmail(user, code);
                return Result.Success();
            }
            // Return Error If Exists
            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code , error.Description , StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            //Check if User is exists
            if(await _userManager.FindByIdAsync(request.UserId) is not { } user)
                return Result.Failure(UserErrors.InvalidCode);

            // Check if User Email is already confirmed
            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            var code = request.Code;

            // Decode the token from Base64 URL-safe format
            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Result.Failure(UserErrors.InvalidCode);
            }

            // Confirm Email
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                // Assign Member Role (Default Role) To Anyone Who Confirmed His Email After Register
                await _userManager.AddToRoleAsync(user, DefaultRoles.Member);
                return Result.Success();
            }

            // Return Error If Exists
            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request) 
        {
            //Check if User is exists
            if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success();

            // Check if User Email is already confirmed
            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            // Generate a secure email confirmation token for the user

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Encode the token using Base64 URL-safe format to make it valid inside a URL

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Email confirmation code:{Code}", code);

            // Send Email Confirmation
            await SendConfirmationEmail(user, code);

            return Result.Success();

        }

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            // Find User By Email
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) 
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            // Check if User is Disabled
            if (user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

            // Check Password 
            var result = await _signInManager.PasswordSignInAsync(user, password, false,true);

            if (result.Succeeded) 
            {

                var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);
                // Generate JWT Token
                var (token, expiresIn) = _jwtProvider.GenerateToken(user , userRoles , userPermissions);

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
                var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);
                return Result.Success(response);
            }

            var error = result.IsNotAllowed 
                ? UserErrors.EmailNotConfirmed
                : result.IsLockedOut
                ? UserErrors.LockedUser
                : UserErrors.InvalidCredentials;

            return Result.Failure<AuthResponse>(error);
        }
        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            // Get UserId
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

            

            // Get User By userId
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

            // Check if User is Disabled
            if (user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

            // Check if User is Locked
            if (user.LockoutEnd > DateTime.UtcNow)
                return Result.Failure<AuthResponse>(UserErrors.LockedUser);

            // Get User Refresh token
            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;

            var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

            // Generate New JWT Token
            var (newToken, expiresIn) = _jwtProvider.GenerateToken(user , userRoles , userPermissions);

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

        public async Task<Result> SendResetPasswordCodeAsync(string email)
        {
            if(await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Success();

            if (!user.EmailConfirmed)
                return Result.Failure(UserErrors.EmailNotConfirmed);
            // Generate a secure password reset token for the user

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the token using Base64 URL-safe format to make it valid inside a URL

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Reset Password confirmation code:{Code}", code);

            // Send Reset Password Confirmation Email
            await SendResetPasswordEmail(user, code);

            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !user.EmailConfirmed)
                return Result.Failure(UserErrors.InvalidCode);

            IdentityResult result;

            try
            {
                // Decode the token from Base64 URL-safe format
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

                // Reset Password
                result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
            }
            catch (FormatException)
            {
                // Invalid Code Format
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
        }



        // Generate Refresh Token
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        // Send Confirmation Email
        private async Task SendConfirmationEmail(ApplicationUser user, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

            var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
                templateModel: new Dictionary<string, string>
                {
                { "{{name}}", user.FirstName },
                    { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
                }
            );

            // Background Job To Send Email Confirmation
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Email Confirmation", emailBody));

            
            await Task.CompletedTask;

        }

        

        private async Task SendResetPasswordEmail(ApplicationUser user, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

            var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
                templateModel: new Dictionary<string, string>
                {
                { "{{name}}", user.FirstName },
                    { "{{action_url}}", $"{origin}/auth/forgetPassword?email={user.Email}&code={code}" }
                }
            );

            // Background Job To Send Reset Password Confirmation Email
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Change Password", emailBody));


            await Task.CompletedTask;

        }
        private async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            //var userPermissions = await _context.Roles
            //    .Join(_context.RoleClaims,
            //        role => role.Id,
            //        claim => claim.RoleId,
            //        (role, claim) => new { role, claim }
            //    )
            //    .Where(x => userRoles.Contains(x.role.Name!))
            //    .Select(x => x.claim.ClaimValue!)
            //    .Distinct()
            //    .ToListAsync(cancellationToken);

             
            //  Get Permissions By LINQ Query Syntax  
            var userPermissions = await (from r in _context.Roles
                                         join p in _context.RoleClaims
                                         on r.Id equals p.RoleId
                                         where userRoles.Contains(r.Name!)
                                         select p.ClaimValue!)
                                         .Distinct()
                                         .ToListAsync(cancellationToken);

            return (userRoles, userPermissions);
        }


    }
}
