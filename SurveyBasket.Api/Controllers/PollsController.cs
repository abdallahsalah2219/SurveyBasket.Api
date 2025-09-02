namespace SurveyBasket.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
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
        var poll = await _pollService.GetAsync(id, cancellationToken);
        if (poll is null)
            return NotFound();

        var response = poll.Adapt<PollResponse>();

        return Ok(response);
    }

    [HttpPost("")]
    public async Task <IActionResult> Add([FromBody] PollRequest request , CancellationToken cancellationToken)
    {
        var newPoll = await _pollService.AddAsync(request.Adapt<Poll>(), cancellationToken);
        //CreatedAtAction()
        // StatusCode is 201 
        // this method helps front end developer to know where this New Object location is by gave him the URL OF this Object

        return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll);

    }

    [HttpPut("{id}")]
    public async Task <IActionResult> Update([FromBody] PollRequest request, [FromRoute] int id
        , CancellationToken cancellationToken)
    {
        var isUpdated =await _pollService.UpdateAsync(request.Adapt<Poll>(), id,cancellationToken);
        if (!isUpdated)
            return NotFound();
        // NoContent()
        // StatusCode is 204
        // The best choice for Update Endpoint
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id,CancellationToken cancellationToken)
    {
        var isDeleted = await _pollService.DeleteAsync(id,cancellationToken);
        if (!isDeleted)
            return NotFound();
        return NoContent();
    }

    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish( [FromRoute] int id , CancellationToken cancellationToken)
    {
        var isPublished = await _pollService.TogglePublishStatusAsync( id, cancellationToken);
        if (!isPublished)
            return NotFound();
        // NoContent()
        // StatusCode is 204
        // The best choice for Update Endpoint
        return NoContent();
    }

}



