using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.OrganizationUsers.Command
{
    public class RemoveOrganizationMemberCommand : IRequest<Response<bool>>
    {
        public Guid OrganizationId { get; set; }
        public Guid TargetUserId { get; set; }

        public class RemoveOrganizationMemberCommandHandler : IRequestHandler<RemoveOrganizationMemberCommand, Response<bool>>
        {
            private readonly IOrganizationUsersRepositoryAsync _orgUserRepository;
            private readonly IAuthenticatedUserService _authenticatedUser;
            private readonly IOrganizationPermissionService _permissionService;

            public RemoveOrganizationMemberCommandHandler(IOrganizationUsersRepositoryAsync orgUserRepository,
                                              IAuthenticatedUserService authenticatedUser,
                                              IOrganizationPermissionService permissionService)
            {
                _orgUserRepository = orgUserRepository;
                _authenticatedUser = authenticatedUser;
                _permissionService = permissionService;
            }

            public async Task<Response<bool>> Handle(RemoveOrganizationMemberCommand command, CancellationToken cancellationToken)
            {
                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    //await _permissionService.EnsureCanRemoveMemberAsync(_authenticatedUser.UserId, command.OrganizationId, command.TargetUserId);

                    var membership = await _orgUserRepository.GetByUserIdAndOrgIdAsync(command.TargetUserId, command.OrganizationId)
                                    ?? throw new ApiException("Member could not be found in this organization.");

                    // Soft delete — keeps record for audit trail
                    membership.IsActive = false;
                    membership.IsDeleted = true;

                    await _orgUserRepository.UpdateAsync(membership);

                    ts.Complete();

                    return new Response<bool>(true, "Member removed successfully.");

                }
            }
        }
    }
}

