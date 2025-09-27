
using Microsoft.AspNetCore.Authorization;

namespace SurveyBasket.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]

public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;


    [HttpGet("")]
    public async Task <IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var polls = await _pollService.GetAllAsync(cancellationToken);

        var response = polls.Adapt<IEnumerable<PollResponse>>();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] int id , CancellationToken cancellationToken)
    {
        var result = await _pollService.GetAsync(id, cancellationToken);


        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.Code, detail: result.Error.Description);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var newPoll = await _pollService.AddAsync(request, cancellationToken);
        //CreatedAtAction()
        // StatusCode is 201 
        // this method helps front end developer to know where this New Object location is by gave him the URL OF this Object

        return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] PollRequest request, [FromRoute] int id
        , CancellationToken cancellationToken)
    {
        var result = await _pollService.UpdateAsync(request, id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Problem(statusCode:StatusCodes.Status404NotFound,title:result.Error.Code,detail:result.Error.Description);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.DeleteAsync(id, cancellationToken);
       
        return result.IsSuccess
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.Code, detail: result.Error.Description);
    }

    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        // NoContent()
        // StatusCode is 204
        // The best choice for Update Endpoint
        return result.IsSuccess
            ? NoContent() 
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.Code, detail: result.Error.Description);
    }

}



