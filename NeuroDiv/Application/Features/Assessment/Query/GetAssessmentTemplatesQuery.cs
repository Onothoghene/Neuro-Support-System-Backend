using Application.DTOs.Assessment;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessment.Query
{
    public class GetAssessmentTemplatesQuery : IRequest<Response<List<AssessmentTemplateSummaryVM>>>
    {
        public Guid? OrganizationId { get; set; }
        public bool? IsSystemTemplate { get; set; }
        public bool? IsActive { get; set; }
        public string? TargetCondition { get; set; }

        public class GetAssessmentTemplatesQueryHandler(IAssessmentTemplateRepositoryAsync assessmentTemplateRepository,
                                                        IAuthenticatedUserService authenticatedUser,
                                                        IMapper mapper) 
              : IRequestHandler<GetAssessmentTemplatesQuery, Response<List<AssessmentTemplateSummaryVM>>>
        {
            private readonly IAssessmentTemplateRepositoryAsync _assessmentTemplateRepository = assessmentTemplateRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<List<AssessmentTemplateSummaryVM>>> Handle(GetAssessmentTemplatesQuery query, CancellationToken cancellationToken)
            {
                var therapistId = Guid.Parse(_authenticatedUser.UserId);

                var templates = await _assessmentTemplateRepository.GetAllAsync(query.OrganizationId, 
                                                                                therapistId, query.IsSystemTemplate,
                                                                                query.IsActive, query.TargetCondition);

                var result = _mapper.Map<List<AssessmentTemplateSummaryVM>>(templates);

                return new Response<List<AssessmentTemplateSummaryVM>>(result, $"{result.Count} template(s) found.");
            }
        }
    }
}
