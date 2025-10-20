namespace SurveyBasket.Api.Contracts.Questions;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    public QuestionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .Length(3, 1000);

        RuleFor(x => x.Answers)
            .NotNull();

        RuleFor(x => x.Answers)
            .Must(x => x.Count > 1)
            .WithMessage("Question Should has at least 2 Answers")
            .When(x => x.Answers != null);

        
        RuleFor(x => x.Answers)
            .Must(x => x.Distinct().Count() == x.Count )
            .WithMessage("You Cannot Duplicated Answers for The Same Question ")
            .When(x => x.Answers != null);
    }
}
