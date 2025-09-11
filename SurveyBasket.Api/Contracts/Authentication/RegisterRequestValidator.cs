namespace SurveyBasket.Api.Contracts.Authentication
{
    public class RegisterRequestValidator:AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator() 
        {
            // Email must be provided and valid format
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            // Password must be provided and at least 6 characters
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            // ConfirmPassword must match Password
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            // FirstName must be provided
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            // LastName must be provided
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            // PhoneNumber is optional but if provided must be a valid phone format
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{7,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format.");
        }
    }
}
