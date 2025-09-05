namespace SurveyBasket.Api.Services.PollService;


public class PollService(ApplicationDbContext context) : IPollService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Polls.AsNoTracking().ToListAsync(cancellationToken);


    public async Task<Poll?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Polls.FindAsync(id,cancellationToken);

    public async Task<Poll> AddAsync(Poll poll , CancellationToken cancellationToken = default)
    {
        await _context.Polls.AddAsync(poll,cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return poll;
    }

    public async Task<bool> UpdateAsync(Poll poll, int id, CancellationToken cancellationToken = default)
    {
        var currentPoll = await GetAsync(id, cancellationToken);
        if (currentPoll is null)
            return false;
        currentPoll.Title = poll.Title;
        currentPoll.Summary = poll.Summary;
        currentPoll.StartAt = poll.StartAt;
        currentPoll.EndsAt = poll.EndsAt;

        await _context.SaveChangesAsync(cancellationToken);
        return true;

    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll =await GetAsync(id, cancellationToken);
        if (poll is null)
            return false;
         _context.Polls.Remove(poll);
        await _context.SaveChangesAsync(cancellationToken);
        return true;

    }

    public async Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var Poll = await GetAsync(id, cancellationToken);
        if (Poll is null)
            return false;
        Poll.IsPublished = !Poll.IsPublished;
   

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
