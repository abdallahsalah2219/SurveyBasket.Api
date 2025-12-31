using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Api.Helpers;

namespace SurveyBasket.Api.Services.NotificationService;

public class NotificationService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
   IHttpContextAccessor httpContextAccessor,
   IEmailSender emailSender
    ) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task SendNewPollsNotification(int? pollId = null)
    {

        IEnumerable<Poll> polls = [];

        if (pollId.HasValue)
        {
            // Get the specific poll by Id if provided
            var poll = await _context.Polls
                .SingleOrDefaultAsync(x => x.Id == pollId && x.IsPublished);

            polls = [poll!];

        }

        else
        {
            // Get all polls that are published and start today
            polls = await _context.Polls
                .Where(x => x.IsPublished && x.StartAt == DateOnly.FromDateTime(DateTime.UtcNow))
                .AsNoTracking()
                .ToListAsync();
        }

        //  Select members only 
        var users = await _userManager.GetUsersInRoleAsync(DefaultRoles.Member);

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach (var poll in polls)
        {
            foreach (var user in users)
            {
                var placeholders = new Dictionary<string, string>
                {
                    { "{{name}}", user.FirstName },
                    { "{{pollTill}}", poll.Title },
                    { "{{PollSummary}}", poll.Summary },
                    { "{{endDate}}", poll.EndsAt.ToString()},
                    { "{{url}}", $"{origin}/polls/start/{poll.Id}"},
                };

                var body = EmailBodyBuilder.GenerateEmailBody("PollNotification", placeholders);

                await _emailSender.SendEmailAsync(user.Email!, $"📣 Survey Basket:{poll.Title}", body);
            }
        }
    }
}
