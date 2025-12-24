
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace SurveyBasket.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
//[Authorize]

public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HasPermission(Permissions.GetPolls)]
    [HttpGet("")]
    public async Task <IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _pollService.GetAllAsync(cancellationToken));
    } 

    [HttpGet("get-current-polls")]
    [Authorize(Roles=DefaultRoles.Member)]
    [EnableRateLimiting("userLimit")]
    public async Task <IActionResult> GetCurrentPolls(CancellationToken cancellationToken)
    {
        return Ok(await _pollService.GetCurrentPollsAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.GetPolls)]

    public async Task<IActionResult> Get([FromRoute] int id , CancellationToken cancellationToken)
    {
        var result = await _pollService.GetAsync(id, cancellationToken);


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.AddPolls)]

    public async Task<IActionResult> Add([FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.AddAsync(request, cancellationToken);
        //CreatedAtAction()
        // StatusCode is 201 
        // this method helps front end developer to know where this New Object location is by gave him the URL OF this Object
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
        

    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> Update([FromBody] PollRequest request, [FromRoute] int id
        , CancellationToken cancellationToken)
    {
        var result = await _pollService.UpdateAsync(request, id, cancellationToken);

         return result.IsSuccess? NoContent() : result.ToProblem();
            

        
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeletePolls)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.DeleteAsync(id, cancellationToken);
       
        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    [HttpPut("{id}/togglePublish")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        // NoContent()
        // StatusCode is 204
        // The best choice for Update Endpoint
        return result.IsSuccess
            ? NoContent() 
            : result.ToProblem();
    }

}



