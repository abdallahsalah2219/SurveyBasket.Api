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

    [HttpPost("")]
    public async Task<IActionResult> LoginAsync(LoginRequest request ,CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult is null ? BadRequest("Invalid Login") : Ok(authResult);  
    }

}
