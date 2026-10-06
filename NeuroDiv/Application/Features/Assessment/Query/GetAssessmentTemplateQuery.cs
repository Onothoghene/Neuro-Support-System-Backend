using Application.DTOs.Assessment;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessment.Query
{
    public class GetAssessmentTemplateQuery : IRequest<Response<AssessmentTemplateVM>>
    {
        public Guid Id { get; set; }

        public class GetAssessmentTemplateQueryHandler(IAssessmentTemplateRepositoryAsync assessmentTemplateRepository,
                                                 IAuthenticatedUserService authenticatedUser,
                                                 IMapper mapper) 
              : IRequestHandler<GetAssessmentTemplateQuery, Response<AssessmentTemplateVM>>
        {
            private readonly IAssessmentTemplateRepositoryAsync _assessmentTemplateRepository = assessmentTemplateRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<AssessmentTemplateVM>> Handle(GetAssessmentTemplateQuery query, CancellationToken cancellationToken)
            {
                var template = await _assessmentTemplateRepository.GetByIdWithDetailsAsync(query.Id) ?? 
                               throw new ApiException("Assessment template could not be found.");

                var result = _mapper.Map<AssessmentTemplateVM>(template);

                return new Response<AssessmentTemplateVM>(result, "Assessment template retrieved successfully.");
            }
        }
    }
}
