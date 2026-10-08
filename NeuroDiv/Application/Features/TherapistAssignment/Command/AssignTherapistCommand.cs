using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.TherapistAssignment.Command
{
    public class AssignTherapistCommand : IRequest<Response<bool>>
    {
        public Guid ChildProfileId { get; set; }
        public Guid TherapistId { get; set; }
        public AssignmentRole Role { get; set; } = AssignmentRole.Secondary;

        public class AssignTherapistCommandHandler(IMapper mapper, IAuthenticatedUserService authenticatedUser,
                                             IChildProfileRepositoryAsync childProfileRepository,
                                             IChildTherapistAssignmentRepositoryAsync assignmentRepository)
                     : IRequestHandler<AssignTherapistCommand, Response<bool>>
        {
            private readonly IChildProfileRepositoryAsync _childProfileRepository = childProfileRepository;
            private readonly IChildTherapistAssignmentRepositoryAsync _assignmentRepository = assignmentRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<bool>> Handle(AssignTherapistCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var child = await _childProfileRepository.GetByIdAsync(command.ChildProfileId) ??
                             throw new ApiException("Child profile could not be found.");

                // Check this therapist isn't already actively assigned
                var existingAssignment = await _assignmentRepository.GetActiveAssignmentAsync(command.ChildProfileId, command.TherapistId);

                if (existingAssignment != null)
                    throw new ApiException("This therapist is already assigned to this child.");

                var assignment = new ChildTherapistAssignment
                {
                    ChildProfileId = command.ChildProfileId,
                    TherapistId = command.TherapistId,
                    Role = command.Role,
                    StartDate = DateTime.UtcNow,
                };

                await _assignmentRepository.AddAsync(assignment);

                ts.Complete();

                return new Response<bool>(true, "Therapist assigned successfully.");
            }
        }
    }
}

