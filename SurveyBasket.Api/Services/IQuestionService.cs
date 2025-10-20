using SurveyBasket.Api.Contracts.Questions;

namespace SurveyBasket.Api.Services;

public interface IQuestionService
{
    Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default);

    Task<Result<QuestionResponse>> GetAsync(int pollId, int questionId, CancellationToken cancellationToken = default);

    Task<Result<QuestionResponse>> AddAsync(int pollId , QuestionRequest request , CancellationToken cancellationToken=default);

    Task<Result> UpdateAsync(int pollId, int questionId, QuestionRequest request, CancellationToken cancellationToken = default);
    Task<Result> ToggleActiveStatusAsync(int pollId , int questionId , CancellationToken cancellationToken=default);

}
