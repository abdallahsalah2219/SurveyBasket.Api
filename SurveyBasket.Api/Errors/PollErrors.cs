namespace SurveyBasket.Api.Errors;

// Use record not class cause i can change change (code , description , StatusCode) in everywhere i use this record
public record PollErrors
{
    public static readonly Error PollNotFound =
        new("Poll.NotFound", "No Poll was Found with given Id", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedPollTitle =
        new("Poll.DuplicatedPollTitle", " Poll with the same Title is Already Exist", StatusCodes.Status409Conflict);

    public static readonly Error NotPublishedOrExpired =
        new("Poll.NotPublishedOrExpired", " Poll with this id Is Not Published or Is Expired ", StatusCodes.Status404NotFound);
}
