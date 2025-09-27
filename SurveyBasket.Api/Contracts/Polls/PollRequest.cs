namespace SurveyBasket.Api.Contracts.Polls
{
    public record PollRequest(
        string Title,
        string Summary,
        DateOnly StartAt,
        DateOnly EndsAt
        );

}
