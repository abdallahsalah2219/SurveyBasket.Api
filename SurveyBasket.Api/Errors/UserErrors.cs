namespace SurveyBasket.Api.Errors
{
    public static class UserErrors
    {
        public static readonly Error InvalidCredentials = 
            new("User.InvalidCredentials", "Invalid email/password");
        
        public static readonly Error InvalidJwtToken = 
            new("User.InvalidJwtToken", "Invalid Jwt token");
        
        public static readonly Error InvalidRefreshToken = 
            new("User.InvalidRefreshToken", "Invalid Refresh token");
        
        public static readonly Error EmailAlreadyExist = 
            new("User.EmailAlreadyExist", "Email Is Already Exist"); 
        
        public static readonly Error InvalidCreateAccount = 
            new("User.InvalidCreateAccount", "Invalid Create Account");
    }
}
