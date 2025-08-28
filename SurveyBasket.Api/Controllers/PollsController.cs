

namespace SurveyBasket.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;
  

    [HttpGet("")]
    public IActionResult GetAll()
    {
        var polls = _pollService.GetAll();
        var response = polls.Adapt<IEnumerable<PollResponse>>();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult Get([FromRoute] int id)
    {
        var poll = _pollService.Get(id);
        if (poll is null)
            return NotFound();

        var response = poll.Adapt<PollResponse>();

        return Ok(response);
    }

    [HttpPost("")]
    public IActionResult Add([FromBody] CreatePollRequest request)
    {
        var newPoll = _pollService.Add(request.Adapt<Poll>());
        //CreatedAtAction()
        // StatusCode is 201 
        // this method helps front end developer to know where this New Object location is by gave him the URL OF this Object

        return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll);
        
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromBody] CreatePollRequest request, [FromRoute] int id)
    {
        var isUpdated = _pollService.Update(request.Adapt<Poll>(), id);
        if (!isUpdated)
            return NotFound();
        // NoContent()
        // StatusCode is 204
        // The best choice for Update Endpoint
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var isDeleted = _pollService.Delete(id);
        if (!isDeleted)
            return NotFound();
        return NoContent();
    }

}



