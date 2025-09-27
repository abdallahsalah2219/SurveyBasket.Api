namespace SurveyBasket.Api.Services.PollService
{
    public interface IPollService
    {
        Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<PollResponse>> GetAsync(int id , CancellationToken cancellationToken = default);
        Task<PollResponse> AddAsync(PollRequest request, CancellationToken cancellationToken =default);
        Task<Result> UpdateAsync(PollRequest poll,int id, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
    }
}
