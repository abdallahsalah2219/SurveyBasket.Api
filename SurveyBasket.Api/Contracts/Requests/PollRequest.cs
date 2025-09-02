

namespace SurveyBasket.Api.Contracts.Requests
{
    public record PollRequest(
        string Title,
        string Summary,
        bool IsPublished,
        DateOnly StartAt,
        DateOnly EndsAt

        );

}
