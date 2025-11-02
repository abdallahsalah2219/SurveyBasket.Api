namespace SurveyBasket.Api.Services.PollService
{
    public interface IPollService
    {
        Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<PollResponse>> GetCurrentPollsAsync(CancellationToken cancellationToken = default);
        Task<Result<PollResponse>> GetAsync(int id , CancellationToken cancellationToken = default);
        Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken =default);
        Task<Result> UpdateAsync(PollRequest request,int id, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
    }
}
