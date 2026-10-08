using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.OrganizationUsersInvite.Command
{
    public class DeleteOrganizationUsersInviteCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }

        public class DeleteOrganizationUsersInviteCommandHandler : IRequestHandler<DeleteOrganizationUsersInviteCommand, Response<bool>>
        {
            private readonly IAuthenticatedUserService _user;
            private readonly IOrganizationUsersInviteRepositoryAsync _organizationUsersInviteRepository;

            public DeleteOrganizationUsersInviteCommandHandler(IAuthenticatedUserService user,
                                                               IOrganizationUsersInviteRepositoryAsync organizationUsersInviteRepository)
            {
                _user = user;
                _organizationUsersInviteRepository = organizationUsersInviteRepository;
            }

            public async Task<Response<bool>> Handle(DeleteOrganizationUsersInviteCommand command, CancellationToken cancellationToken)
            {
                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var invite = await _organizationUsersInviteRepository.GetByIdAsync(command.Id) ?? 
                                                            throw new ApiException($"The requested organization user invite could not be found.");

                    // Can't delete an invite that has already been accepted —
                    // the user is already a member at that point
                    if (invite.IsAccepted)
                        throw new ApiException("This invitation has already been accepted and cannot be deleted.");
                    
                    // Soft delete — preserves the record for audit trail
                    invite.IsDeleted = true;

                    await _organizationUsersInviteRepository.UpdateAsync(invite);

                    ts.Complete();

                    return new Response<bool>(true, "Organization user invite deleted successfully.");

                }
            }
        }
    }
}

