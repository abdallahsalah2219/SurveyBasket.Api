

using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Services;

namespace SurveyBasket.Api.Controllers;

[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;

    [HttpGet("")]

    public async Task<IActionResult> GetAll([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetAllAsync(pollId, cancellationToken);

       return result.IsSuccess ? Ok(result.Value) 
            : result.ToProblem(StatusCodes.Status404NotFound);

    }

    [HttpGet("{questionId}")]
    public async Task<IActionResult> Get([FromRoute] int pollId, [FromRoute] int questionId ,CancellationToken cancellationToken)
    {
        var result = await _questionService.GetAsync(pollId, questionId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value)
            : result.ToProblem(StatusCodes.Status404NotFound);
    }

    [HttpPost("")]

    public async Task<IActionResult> Add([FromRoute] int pollId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.AddAsync(pollId, request, cancellationToken);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(Get), new { pollId = pollId, id = result.Value.Id } , result.Value);

        return result.Error.Equals(QuestionErrors.DuplicatedQuestionContent)
             ? result.ToProblem(StatusCodes.Status409Conflict)
             : result.ToProblem(StatusCodes.Status404NotFound);
    }
    [HttpPut("{questionId}")]

    public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int questionId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.UpdateAsync(pollId , questionId , request , cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return result.Error.Equals(QuestionErrors.DuplicatedQuestionContent)
             ? result.ToProblem(StatusCodes.Status409Conflict)
             : result.ToProblem(StatusCodes.Status404NotFound);
    }

    [HttpPut("{questionId}/toggleStatus")]
    public async Task<IActionResult> ToggleStatus([FromRoute] int pollId, [FromRoute] int questionId, CancellationToken cancellationToken)
    {
        var result = await _questionService.ToggleActiveStatusAsync(pollId, questionId, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem(StatusCodes.Status404NotFound);
    }
}
