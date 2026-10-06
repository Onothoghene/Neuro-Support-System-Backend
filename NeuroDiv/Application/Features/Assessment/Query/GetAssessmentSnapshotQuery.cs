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
    public class GetAssessmentSnapshotQuery : IRequest<Response<AssessmentSnapshotSummaryVM>>
    {
        public Guid Id { get; set; }

        public class GetAssessmentSnapshotQueryHandler(IAssessmentSnapshotRepositoryAsync assessmentSnapshotRepository,
                                                       IAuthenticatedUserService authenticatedUser,
                                                       IMapper mapper) 
              : IRequestHandler<GetAssessmentSnapshotQuery, Response<AssessmentSnapshotSummaryVM>>
        {
            private readonly IAssessmentSnapshotRepositoryAsync _assessmentSnapshotRepository = assessmentSnapshotRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<AssessmentSnapshotSummaryVM>> Handle(GetAssessmentSnapshotQuery query, CancellationToken cancellationToken)
            {
                var snapshot = await _assessmentSnapshotRepository.GetByIdWithDetailsAsync(query.Id) ?? 
                               throw new ApiException("Assessment snapshot could not be found.");

                var result = _mapper.Map<AssessmentSnapshotSummaryVM>(snapshot);

                return new Response<AssessmentSnapshotSummaryVM>(result, "Assessment retrieved successfully.");
            }
        }
    }
}
