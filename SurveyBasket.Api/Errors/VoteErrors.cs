namespace SurveyBasket.Api.Errors;

public static class VoteErrors
{
    public static readonly Error InvalidQuestions =
        new("Vote.InvalidQuestions", "Invalid Questions", StatusCodes.Status400BadRequest); 

    public static readonly Error DuplicatedVote =
        new("Vote.DuplicatedVote", " This User Voted on this Poll before", StatusCodes.Status409Conflict);

//    public static readonly Error NotPublishedOrExpired =
//        new("Poll.NotPublishedOrExpired", " Poll with this id Is Not Published or Is Expired ");
}
