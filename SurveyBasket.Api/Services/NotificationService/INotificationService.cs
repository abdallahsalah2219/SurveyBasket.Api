namespace SurveyBasket.Api.Services.NotificationService;

public interface INotificationService
{
    Task SendNewPollsNotification(int? pollId = null);
}
