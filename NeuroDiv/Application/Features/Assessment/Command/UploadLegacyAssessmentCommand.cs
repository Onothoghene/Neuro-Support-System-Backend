using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessment.Command
{
    public class UploadLegacyAssessmentCommand : IRequest<Response<Guid>>
    {
        public Guid AssessmentTemplateId { get; set; }
        public Guid ChildProfileId { get; set; }
        public Guid? SessionOccurrenceId { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string UploadedFilePath { get; set; }
        public string? ClinicalNotes { get; set; }

        public class UploadLegacyAssessmentCommandHandler(IAssessmentSnapshotRepositoryAsync snapshotRepository,
                                                          IAuthenticatedUserService authenticatedUser,
                                                          IMapper mapper)
              : IRequestHandler<UploadLegacyAssessmentCommand, Response<Guid>>
        {
            private readonly IAssessmentSnapshotRepositoryAsync _snapshotRepository = snapshotRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<Guid>> Handle(UploadLegacyAssessmentCommand command, CancellationToken cancellationToken)
            {
                var therapistId = Guid.Parse(_authenticatedUser.UserId);

                // Create snapshot with no responses yet —
                // AI extraction will populate responses in the integrations phase
                // Therapist verifies after AI extraction
                var snapshot = new AssessmentSnapshot
                {
                    AssessmentTemplateId = command.AssessmentTemplateId,
                    ChildProfileId = command.ChildProfileId,
                    SessionOccurrenceId = command.SessionOccurrenceId,
                    AssessmentDate = command.AssessmentDate,
                    TotalScore = 0,          // set after AI extraction
                    IsLegacyUpload = true,
                    UploadedFilePath = command.UploadedFilePath,
                    IsVerified = false,       // pending AI + human verification
                    ClinicalNotes = command.ClinicalNotes,
                };

                var result = await _snapshotRepository.AddAsync(snapshot);

                return new Response<Guid>(result.Id,
                    "Assessment uploaded successfully. It will be processed and ready for verification shortly.");
            }
        }
    }
}