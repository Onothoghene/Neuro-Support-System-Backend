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
    public class GetAssessmentSnapshotsQuery : IRequest<Response<List<AssessmentSnapshotSummaryVM>>>
    {
        public Guid ChildProfileId { get; set; }
        public Guid? TemplateId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public class GetAssessmentSnapshotsQueryHandler(IAssessmentSnapshotRepositoryAsync assessmentSnapshotRepository,
                                                        IAuthenticatedUserService authenticatedUser,
                                                        IMapper mapper) 
              : IRequestHandler<GetAssessmentSnapshotsQuery, Response<List<AssessmentSnapshotSummaryVM>>>
        {
            private readonly IAssessmentSnapshotRepositoryAsync _assessmentSnapshotRepository = assessmentSnapshotRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<List<AssessmentSnapshotSummaryVM>>> Handle(GetAssessmentSnapshotsQuery query, CancellationToken cancellationToken)
            {
                var therapistId = Guid.Parse(_authenticatedUser.UserId);

                var snapshots = await _assessmentSnapshotRepository.GetAllByChildIdAsync(query.ChildProfileId, query.TemplateId,
                                                                                         query.FromDate, query.ToDate);

                var result = _mapper.Map<List<AssessmentSnapshotSummaryVM>>(snapshots);

                return new Response<List<AssessmentSnapshotSummaryVM>>(result, $"{result.Count} snapshot(s) found.");
            }
        }
    }
}
