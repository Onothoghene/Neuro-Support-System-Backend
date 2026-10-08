using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.TherapistAssignment.Command
{
    public class RemoveTherapistAssignmentCommand : IRequest<Response<bool>>
    {
        public Guid ChildProfileId { get; set; }
        public Guid TherapistId { get; set; }

        public class RemoveTherapistAssignmentCommandHandler : IRequestHandler<RemoveTherapistAssignmentCommand, Response<bool>>
        {
            private readonly IChildTherapistAssignmentRepositoryAsync _assignmentRepository;
            private readonly IAuthenticatedUserService _authenticatedUser;

            public RemoveTherapistAssignmentCommandHandler(IChildTherapistAssignmentRepositoryAsync assignmentRepository,
                                                           IAuthenticatedUserService authenticatedUser)
            {
                _assignmentRepository = assignmentRepository;
                _authenticatedUser = authenticatedUser;
            }

            public async Task<Response<bool>> Handle(RemoveTherapistAssignmentCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var assignment = await _assignmentRepository.GetActiveAssignmentAsync(command.ChildProfileId, command.TherapistId)
                                 ?? throw new ApiException("Active assignment could not be found.");

                // Set EndDate — this is the only time EndDate gets filled
                assignment.EndDate = DateTime.UtcNow;

                await _assignmentRepository.UpdateAsync(assignment);

                ts.Complete();

                return new Response<bool>(true, "Therapist removed from child successfully.");
            }

        }
    }
}

