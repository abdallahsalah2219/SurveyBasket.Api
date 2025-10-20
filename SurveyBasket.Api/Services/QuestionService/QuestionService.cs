﻿using Microsoft.AspNetCore.Http.HttpResults;
using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Entities;
using System.Collections.Generic;

namespace SurveyBasket.Api.Services.QuestionService;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);
        if (!pollIsExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);



        var questions = await _context.Questions
            .Where(x => x.PollId == pollId)
            .Include(x => x.Answers)
            //.Select(q=> new QuestionResponse(
            //    q.Id,
            //    q.Content,
            //    q.Answers.Select(a => new AnswerResponse(a.Id, a.Content ))
            //    ))
            .ProjectToType<QuestionResponse>()
            .AsNoTracking().ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(questions);

    }

    public async Task<Result<QuestionResponse>> GetAsync(int pollId, int questionId, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);
        if (!pollIsExists)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var question = await _context.Questions
            .Where(x => x.PollId == pollId && x.Id == questionId)
            .Include(x => x.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

        return Result.Success(question);

    }
    public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);
        if (!pollIsExists)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var questionIsExists = await _context.Questions.AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken: cancellationToken);

        if (questionIsExists)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        var question = request.Adapt<Question>();

        question.PollId = pollId;



        await _context.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());
    }

    public async Task<Result> UpdateAsync(int pollId, int questionId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var questionIsExists = await _context.Questions
            .AnyAsync(x => x.PollId == pollId
                     && x.Id != questionId
                     && x.Content == request.Content,
                     cancellationToken
                     );

        if (questionIsExists)
            return Result.Failure(QuestionErrors.DuplicatedQuestionContent);

        var question = await _context.Questions
            .Include(x => x.Answers)
            .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == questionId, cancellationToken);

        if(question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.Content = request.Content;

        // Get Current Answer in Database
        var currentAnswers = question.Answers.Select(x=>x.Content).ToList();

        // Add New Answers If I want

        var newAnswers =request.Answers.Except(currentAnswers).ToList();

        newAnswers.ForEach(answer =>
        {
            question.Answers.Add(new Answer { Content = answer });
        });

        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleActiveStatusAsync(int pollId, int questionId, CancellationToken cancellationToken = default)
    {


        var question = await _context.Questions.SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == questionId, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


}
