namespace SurveyBasket.Api.Contracts.Users;

public record UpdateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    IList<string> Roles
);

