namespace SurveyBasket.Api.Errors;


// Use record not class cause i can change change (code , description , StatusCode) in everywhere i use this record
// like this  if (!user.EmailConfirmed)
// return Result.Failure(UserErrors.EmailNotConfirmed with {StatusCode = StatusCodes.Status400BadRequest });
// although EmailNotConfirmed return Status401Unauthorized but i change it to Status400BadRequest

public record UserErrors
{
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Invalid email/password", StatusCodes.Status401Unauthorized);

    public static readonly Error DisabledUser =
        new("User.DisabledUser", "Disabled user , please contact administrator", StatusCodes.Status401Unauthorized);

    public static readonly Error LockedUser =
        new("User.LockedUser", "Locked user , please contact administrator", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidJwtToken =
        new("User.InvalidJwtToken", "Invalid Jwt token", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Invalid Refresh token", StatusCodes.Status401Unauthorized);

    public static readonly Error EmailAlreadyExist =
        new("User.EmailAlreadyExist", "Email Is Already Exist", StatusCodes.Status409Conflict);

    public static readonly Error InvalidCreateAccount =
        new("User.InvalidCreateAccount", "Invalid Create Account", StatusCodes.Status400BadRequest);

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Email Is Not Confirmed Yet", StatusCodes.Status401Unauthorized);


    public static readonly Error InvalidCode =
        new("User.InvalidCode", "Invalid code", StatusCodes.Status401Unauthorized);

    public static readonly Error DuplicatedConfirmation =
        new("User.DuplicatedConfirmation", "Email already confirmed", StatusCodes.Status400BadRequest);


    public static readonly Error UserNotFound =
        new("User.UserNotFound", "User with this Id is Not Found", StatusCodes.Status404NotFound);

    public static readonly Error InvalidRoles =
        new("User.InvalidRoles", "Invalid Roles", StatusCodes.Status400BadRequest);
}
