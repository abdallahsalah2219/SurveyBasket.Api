namespace SurveyBasket.Api.Contracts.Authentication;
public record RegisterRequest(
    string Email,
string Password,
string ConfirmPassword,
string FirstName,
string LastName,
string? PhoneNumber = null
    );


