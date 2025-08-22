namespace SurveyBasket.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HttpGet("")]
    public IActionResult GetAll()
    {
        return Ok(_pollService.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var poll = _pollService.Get(id);
        return poll is null ? NotFound() : Ok(poll);
    }

    [HttpPost("")]
    public IActionResult Add(Poll request)
    {
        var newPoll = _pollService.Add(request);
        //CreatedAtAction()
        // StatusCode is 201 
        // this method helps front end developer to know where this New Object location is by gave him the URL OF this Object
        
        return CreatedAtAction(nameof(Get),new {id =newPoll.Id },newPoll);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Poll request, int id)
    {
        var isUpdated = _pollService.Update(request,id);
        if (!isUpdated)
            return NotFound();
        // NoContent()
        // StatusCode is 204
        // The best choice for Update Endpoint
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) 
    {
        var isDeleted = _pollService.Delete(id);
        if (!isDeleted)
            return NotFound();
        return NoContent();
    }

}



