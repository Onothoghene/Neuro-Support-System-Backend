using Application.DTOs.Assessment;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
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
    public class CreateAssessmentTemplateCommand : IRequest<Response<Guid>>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? TargetCondition { get; set; }
        public TemplateVisibility Visibility { get; set; } = TemplateVisibility.Private;
        public Guid? OrganizationId { get; set; }
        public bool HasAutoScoring { get; set; } = true;
        public List<CreateAssessmentSectionRequest> Sections { get; set; } = new();
        public List<CreateScoreRangeRequest> ScoreRanges { get; set; } = new();

        public class CreateAssessmentTemplateCommandHandler(IAssessmentTemplateRepositoryAsync assessmentTemplateRepository,
                                                            IAuthenticatedUserService authenticatedUser,
                                                            IMapper mapper)
              : IRequestHandler<CreateAssessmentTemplateCommand, Response<Guid>>
        {
            private readonly IAssessmentTemplateRepositoryAsync _assessmentTemplateRepository = assessmentTemplateRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<Guid>> Handle(CreateAssessmentTemplateCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var therapistId = Guid.Parse(_authenticatedUser.UserId);

                // Build sections with questions and options
                var sections = _mapper.Map<List<AssessmentSection>>(command.Sections.OrderBy(s => s.DisplayOrder));

                //var sections = command.Sections
                //    .OrderBy(s => s.DisplayOrder)
                //    .Select(s => new AssessmentSection
                //    {
                //        Title = s.Title,
                //        Description = s.Description,
                //        DisplayOrder = s.DisplayOrder,
                //        CreatedBy = _authenticatedUser.UserId,
                //        Created = DateTime.UtcNow,
                //        Questions = s.Questions
                //            .OrderBy(q => q.DisplayOrder)
                //            .Select(q => new AssessmentQuestion
                //            {
                //                Text = q.Text,
                //                HelpText = q.HelpText,
                //                QuestionType = q.QuestionType,
                //                IsRequired = q.IsRequired,
                //                DisplayOrder = q.DisplayOrder,
                //                Options = q.Options
                //                    .OrderBy(o => o.DisplayOrder)
                //                    .Select(o => new AssessmentQuestionOption
                //                    {
                //                        OptionText = o.OptionText,
                //                        Score = o.Score,
                //                        DisplayOrder = o.DisplayOrder,
                //                        CreatedBy = _authenticatedUser.UserId,
                //                        Created = DateTime.UtcNow,
                //                    }).ToList()
                //            }).ToList()
                //    }).ToList();

                // Calculate min/max possible scores
                decimal maxScore = 0;
                foreach (var section in sections)
                    foreach (var question in section.Questions)
                        if (question.QuestionType != QuestionType.Text)
                            maxScore += question.Options.Any()
                                ? question.Options.Max(o => o.Score) : 0;

                var scoreRanges = _mapper.Map<List<AssessmentScoreRange>>(command.ScoreRanges);
                
                var template = _mapper.Map<AssessmentTemplate>(command);
                template.MaxPossibleScore = maxScore;

                var result = await _assessmentTemplateRepository.AddAsync(template);

                ts.Complete();

                return new Response<Guid>(result.Id, "Assessment template created successfully.");
            }
        }
    }
}