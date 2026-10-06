using Application.DTOs.Assessment;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.Assessment.Command
{
    public class SubmitAssessmentCommand : IRequest<Response<Guid>>
    {
        public Guid AssessmentTemplateId { get; set; }
        public Guid ChildProfileId { get; set; }
        public Guid? SessionOccurrenceId { get; set; }
        public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
        public string? ClinicalNotes { get; set; }
        public List<AssessmentResponseRequest> Responses { get; set; } = new();

        public class SubmitAssessmentCommandHandler(IAssessmentTemplateRepositoryAsync assessmentTemplateRepository,
                                                    IAssessmentSnapshotRepositoryAsync snapshotRepository,
                                                    IAuthenticatedUserService authenticatedUser)
              : IRequestHandler<SubmitAssessmentCommand, Response<Guid>>
        {
            private readonly IAssessmentTemplateRepositoryAsync _assessmentTemplateRepository = assessmentTemplateRepository;
            private readonly IAssessmentSnapshotRepositoryAsync _snapshotRepository = snapshotRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;

            public async Task<Response<Guid>> Handle(SubmitAssessmentCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var therapistId = Guid.Parse(_authenticatedUser.UserId);

                var template = await _assessmentTemplateRepository.GetByIdWithDetailsAsync(command.AssessmentTemplateId)
                               ?? throw new ApiException("Assessment template could not be found.");

                // Validate all required questions are answered
                var allQuestions = template.Sections.SelectMany(s => s.Questions).ToList();

                var requiredQuestions = allQuestions.Where(q => q.IsRequired).ToList();

                var answeredQuestionIds = command.Responses.Select(r => r.QuestionId).ToHashSet();

                var unanswered = requiredQuestions.Where(q => !answeredQuestionIds.Contains(q.Id)).ToList();

                if (unanswered.Any())
                    throw new ApiException($"The following required questions are unanswered: " 
                                           + string.Join(", ", unanswered.Select(q => q.Text)));

                // Build responses and calculate scores
                decimal totalScore = 0;
                var responses = new List<AssessmentResponse>();

                foreach (var responseRequest in command.Responses)
                {
                    var question = allQuestions.FirstOrDefault(q => q.Id == responseRequest.QuestionId)?? 
                                   throw new ApiException($"Question {responseRequest.QuestionId} not found in template.");

                    decimal score = 0;
                    Guid? selectedOptionId = null;

                    if (question.QuestionType != QuestionType.Text && responseRequest.SelectedOptionId.HasValue)
                    {
                        var option = question.Options.FirstOrDefault(o => o.Id == responseRequest.SelectedOptionId.Value) ?? 
                                     throw new ApiException($"Invalid option selected for question: {question.Text}");

                        score = option.Score;
                        selectedOptionId = option.Id;
                        totalScore += score;
                    }

                    responses.Add(new AssessmentResponse
                    {
                        AssessmentQuestionId = responseRequest.QuestionId,
                        SelectedOptionId = selectedOptionId,
                        TextResponse = responseRequest.TextResponse,
                        Score = score,
                    });
                }

                // Match score to range label
                var scoreLabel = template.ScoreRanges.FirstOrDefault(r => totalScore >= r.MinScore && 
                                 totalScore <= r.MaxScore)?.Label;

                // Create immutable snapshot
                var snapshot = new AssessmentSnapshot
                {
                    AssessmentTemplateId = command.AssessmentTemplateId,
                    ChildProfileId = command.ChildProfileId,
                    TherapistId = therapistId,
                    SessionOccurrenceId = command.SessionOccurrenceId,
                    AssessmentDate = command.AssessmentDate,
                    TotalScore = totalScore,
                    ScoreLabel = scoreLabel,
                    ClinicalNotes = command.ClinicalNotes,
                    IsLegacyUpload = false,
                    IsVerified = true,   // digital = always verified
                    Responses = responses,
                };

                var result = await _snapshotRepository.AddAsync(snapshot);

                ts.Complete();

                return new Response<Guid>(result.Id, "Assessment submitted successfully.");
            }
        }
    }
}