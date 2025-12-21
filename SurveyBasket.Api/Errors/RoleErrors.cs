    namespace SurveyBasket.Api.Errors
{
    public static class RoleErrors
    {
        public static readonly Error RoleNotFound = 
            new("Role.RoleNotFound", "Role is not Found", StatusCodes.Status404NotFound);

        public static readonly Error RoleAlreadyExists =
            new("Role.RoleAlreadyExists", "Role with this Name Is Already Exists", StatusCodes.Status409Conflict);

        public static readonly Error InvalidPermissions =
            new("Role.InvalidPermissions", "Invalid Permissions", StatusCodes.Status400BadRequest);

        //public static readonly Error EmailAlreadyExist = 
        //    new("User.EmailAlreadyExist", "Email Is Already Exist", StatusCodes.Status409Conflict); 

        //public static readonly Error InvalidCreateAccount = 
        //    new("User.InvalidCreateAccount", "Invalid Create Account", StatusCodes.Status400BadRequest);

        //public static readonly Error EmailNotConfirmed =
        //    new("User.EmailNotConfirmed", "Email Is Not Confirmed Yet", StatusCodes.Status401Unauthorized);


        //public static readonly Error InvalidCode =
        //    new("User.InvalidCode", "Invalid code", StatusCodes.Status401Unauthorized);

        //public static readonly Error DuplicatedConfirmation =
        //    new("User.DuplicatedConfirmation", "Email already confirmed", StatusCodes.Status400BadRequest);
    }
}
