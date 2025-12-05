namespace SurveyBasket.Api.Contracts.Users;

public class UpdateProfileRequestVaidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestVaidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(3, 100);


        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(3, 100);
    }
}
