using SurveyBasket.Api.Contracts.Common;
using SurveyBasket.Api.Contracts.Questions;

namespace SurveyBasket.Api.Controllers;

[Route("api/polls/{pollId}/[controller]")]
[ApiController]

public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;

    [HttpGet("")]
    [HasPermission(Permissions.GetQuestions)]

    public async Task<IActionResult> GetAll([FromRoute] int pollId, [FromQuery] RequestFilters filters, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetAllAsync(pollId, filters, cancellationToken);

       return result.IsSuccess ? Ok(result.Value) 
            : result.ToProblem();

    }

    [HttpGet("{questionId}")]
    [HasPermission(Permissions.GetPolls)]

    public async Task<IActionResult> Get([FromRoute] int pollId, [FromRoute] int questionId ,CancellationToken cancellationToken)
    {
        var result = await _questionService.GetAsync(pollId, questionId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.AddQuestions)]

    public async Task<IActionResult> Add([FromRoute] int pollId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.AddAsync(pollId, request, cancellationToken);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(Get), new { pollId, questionId = result.Value.Id } , result.Value);

       return result.ToProblem() ;
    }
    [HttpPut("{questionId}")]
    [HasPermission(Permissions.UpdateQuestions)]

    public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int questionId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.UpdateAsync(pollId , questionId , request , cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{questionId}/toggleStatus")]
    [HasPermission(Permissions.UpdateQuestions)]

    public async Task<IActionResult> ToggleStatus([FromRoute] int pollId, [FromRoute] int questionId, CancellationToken cancellationToken)
    {
        var result = await _questionService.ToggleActiveStatusAsync(pollId, questionId, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
