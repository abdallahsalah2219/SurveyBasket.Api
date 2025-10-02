using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading;


namespace SurveyBasket.Api.Services.PollService;


public class PollService(ApplicationDbContext context) : IPollService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Polls.AsNoTracking().ToListAsync(cancellationToken);


    public async Task<Result<PollResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);

        return poll is not null
            ? Result.Success(poll.Adapt<PollResponse>())
            : Result.Failure<PollResponse>(PollErrors.PollNotFound);

    }


    public async Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
    {
        var isExistingTitle = await _context.Polls.AnyAsync(x => x.Title == request.Title, cancellationToken: cancellationToken);

        if (isExistingTitle) 
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);

        var poll = request.Adapt<Poll>();

        await _context.AddAsync(poll, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(poll.Adapt<PollResponse>());
    }

    public async Task<Result> UpdateAsync(PollRequest request, int id, CancellationToken cancellationToken = default)
    {

        var isExistingTitle = await _context.Polls.AnyAsync(x => x.Title == request.Title && x.Id != id, cancellationToken: cancellationToken);

        if (isExistingTitle)
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);
         

        var currentPoll = await _context.Polls.FindAsync(id, cancellationToken);

        if (currentPoll is null)
            return Result.Failure(PollErrors.PollNotFound);
        currentPoll.Title = request.Title;
        currentPoll.Summary = request.Summary;
        currentPoll.StartAt = request.StartAt;
        currentPoll.EndsAt = request.EndsAt;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id ,cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();

    }

    public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var Poll = await _context.Polls.FindAsync(id, cancellationToken);
        if (Poll is null)
            return Result.Failure(PollErrors.PollNotFound);
        Poll.IsPublished = !Poll.IsPublished;


        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
