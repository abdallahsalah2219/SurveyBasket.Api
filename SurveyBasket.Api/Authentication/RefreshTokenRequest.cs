namespace SurveyBasket.Api.Authentication;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
    );


