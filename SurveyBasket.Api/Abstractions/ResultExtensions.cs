namespace SurveyBasket.Api.Abstractions;

public static class ResultExtensions
{
    public static ObjectResult ToProblem(this Result result, int statusCode)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot Convert Success Result to a Problem");

        var problem = Results.Problem(statusCode: statusCode);

        var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails;

        problemDetails!.Extensions = new Dictionary<string, object?>
          {
              {
                  "errors" ,new[] { result.Error}
              }
          };

        return new ObjectResult(problemDetails);
    }
}
