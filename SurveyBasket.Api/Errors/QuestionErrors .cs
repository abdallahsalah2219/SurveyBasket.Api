namespace SurveyBasket.Api.Errors;


// Use record not class cause i can change change (code , description , StatusCode) in everywhere i use this record
public record QuestionErrors
{
    public static readonly Error QuestionNotFound =
        new("Question.NotFound", "No Question was Found with given Id", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedQuestionContent =
        new("Question.DuplicatedQuestionContent", " Question with the same Content is Already Exist", StatusCodes.Status409Conflict);
}
