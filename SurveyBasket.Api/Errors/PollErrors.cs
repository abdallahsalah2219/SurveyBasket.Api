namespace SurveyBasket.Api.Errors
{
    public static class PollErrors
    {
        public static readonly Error PollNotFound =
            new("Poll.NotFound", "No Poll was Found with given Id");
    }
}
