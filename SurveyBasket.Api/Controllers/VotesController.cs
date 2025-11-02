using SurveyBasket.Api.Contracts.Votes;
using SurveyBasket.Api.Services.VoteService;

namespace SurveyBasket.Api.Controllers;



[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
public class VotesController(IQuestionService questionService , IVoteService voteService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;
    private readonly IVoteService _voteService = voteService;

    [HttpGet("")]
    public async Task<IActionResult> Start([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var userId =User.GetUserId();

        var result = await _questionService.GetAvailableQuestionsAsync(pollId, userId!, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

        
    }

    [HttpPost("")]

    public async Task<IActionResult> Vote([FromRoute] int pollId, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var userId =  User.GetUserId();

        var result = await _voteService.AddAsync(pollId, userId!, request, cancellationToken);

        return result.IsSuccess
           ? Created() : result.ToProblem();

        
    }
}
