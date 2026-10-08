using Application.Interfaces.Repositories;
using Application.Interfaces;
using Application.Wrappers;
using AutoMapper;
using MediatR;
using System.Threading;
using System.Transactions;
using System.Threading.Tasks;
using Application.Exceptions;
using System;

namespace Application.Features.OrganizationUsersInvite.Command
{
    public class AddOrUpdateOrganizationUsersInviteCommand : IRequest<Response<bool>>
    {
        public Guid? Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid OrganizationRoleId { get; set; }

        public class AddOrUpdateOrganizationUsersInviteCommandHandler : IRequestHandler<AddOrUpdateOrganizationUsersInviteCommand, Response<bool>>
        {
            private readonly IMapper _mapper;
            private readonly IAuthenticatedUserService _user;
            private readonly IOrganizationUsersInviteRepositoryAsync _organizationUsersInviteRepository;
            private readonly IEmailService _emailService;  

            public AddOrUpdateOrganizationUsersInviteCommandHandler(IMapper mapper, IAuthenticatedUserService user,
                                                                    IOrganizationUsersInviteRepositoryAsync organizationUsersInviteRepository,
                                                                    IEmailService emailService)
            {
                _mapper = mapper;
                _user = user;
                _organizationUsersInviteRepository = organizationUsersInviteRepository;
                _emailService = emailService;
            }

            public async Task<Response<bool>> Handle(AddOrUpdateOrganizationUsersInviteCommand command, CancellationToken cancellationToken)
            {
                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    //Update functionality
                    if (command.Id.HasValue && command.Id is not null)
                    {
                        var orgUserInvite = await _organizationUsersInviteRepository.GetByIdAsync(command.Id.Value);
                        
                        if (orgUserInvite == null)
                            throw new ApiException($"The requested organization user invite could not be found.");

                        // Only allow updating a pending invite — not one already accepted
                        if (orgUserInvite.IsAccepted)
                            throw new ApiException("This invitation has already been accepted and cannot be modified.");

                        // If the email changed, generate a fresh token and reset expiry
                        bool emailChanged = !orgUserInvite.Email.Equals(command.Email, StringComparison.OrdinalIgnoreCase);

                        orgUserInvite.FirstName = command.FirstName;
                        orgUserInvite.LastName = command.LastName;
                        orgUserInvite.Email = command.Email;
                        orgUserInvite.OrganizationId = command.OrganizationId;
                        orgUserInvite.OrganizationRoleId = command.OrganizationRoleId;

                        if (emailChanged)
                        {
                           // orgUserInvite.Token = GenerateInviteToken();
                            orgUserInvite.ExpiryDate = DateTime.UtcNow.AddDays(7);
                        }

                        await _organizationUsersInviteRepository.UpdateAsync(orgUserInvite);
                    }
                    //Create Functionality
                    else
                    {
                        // Block duplicate pending invites for the same email + org
                        var existingInvite = await _organizationUsersInviteRepository
                            .GetPendingInviteByEmailAndOrgAsync(command.Email, command.OrganizationId);

                        if (existingInvite != null)
                            throw new ApiException($"A pending invitation for '{command.Email}' already exists for this organization.");

                        var newInvite = _mapper.Map<Domain.Entities.OrganizationUsersInvite>(command);

                      //  newInvite.Token = GenerateInviteToken();
                        newInvite.ExpiryDate = DateTime.UtcNow.AddDays(7);
                        newInvite.IsAccepted = false;

                        await _organizationUsersInviteRepository.AddAsync(newInvite);

                        //await SendInviteEmailAsync(
                        //    newInvite.Email,
                        //    newInvite.FirstName,
                        //    newInvite.Token,
                        //    newInvite.OrganizationId);
                    }

                    ts.Complete();
                }

                return new Response<bool>(true, "Request excuted successfully.");
            }
        }
    }
}

