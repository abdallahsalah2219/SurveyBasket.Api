using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Services.AuthService;

namespace SurveyBasket.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService /*,IOptions<JwtOptions> jwtOptions*/) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    //private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Call the AuthService to register the user
        var authResponse = await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            cancellationToken);

        // If registration failed, return BadRequest
        if (authResponse is null)
        {
            return BadRequest(new { Message = "Registration failed. Email may already be in use." });
        }

        // Return the authentication response on success
        return Ok(authResponse);
    }


    [HttpPost("")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult is null ? BadRequest("Invalid Login") : Ok(authResult);
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return authResult is null ? BadRequest("Invalid Token") : Ok(authResult);
    }

    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var isRevoked = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return isRevoked ? Ok(): BadRequest("Operation Failed") ;
    }
}
