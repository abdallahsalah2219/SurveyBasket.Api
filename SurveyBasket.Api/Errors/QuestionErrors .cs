namespace SurveyBasket.Api.Errors;

public static class QuestionErrors
{
    public static readonly Error QuestionNotFound =
        new("Question.NotFound", "No Question was Found with given Id"); 
    
    public static readonly Error DuplicatedQuestionContent =
        new("Question.DuplicatedQuestionContent", " Question with the same Content is Already Exist");
}
