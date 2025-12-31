namespace SurveyBasket.Api.Contracts.Polls;

public record PollResponse
(
     int Id,
     string Title,
     string Summary,
     bool IsPublished,
     DateOnly StartAt,
     DateOnly EndsAt

);
public record PollResponseV2
(
     int Id,
     string Title,
     string Summary,
     DateOnly StartAt,
     DateOnly EndsAt

);