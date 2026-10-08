using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.Organizations.Command
{
    public class UpdateOrganizationsCommand : IRequest<Response<bool>>
    {
        public Guid OrganizationId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Domain change is restricted to Clinic Owner only.
        /// Leave null to keep existing domain.
        /// </summary>
        public string? Domain { get; set; }

        public class UpdateOrganizationsCommandHandler : IRequestHandler<UpdateOrganizationsCommand, Response<bool>>
        {
            private readonly IMapper _mapper;
            private readonly IOrganizationsRepositoryAsync _organizationsRepository;
            private readonly IAuthenticatedUserService _authenticatedUser;
            private readonly IOrganizationPermissionService _permissionService;

            public UpdateOrganizationsCommandHandler(IMapper mapper, IOrganizationPermissionService permissionService,
                                                     IAuthenticatedUserService authenticatedUser,
                                                     IOrganizationsRepositoryAsync organizationsRepository)
            {
                _mapper = mapper;
                _permissionService = permissionService;
                _authenticatedUser = authenticatedUser;
                _organizationsRepository = organizationsRepository;
            }

            public async Task<Response<bool>> Handle(UpdateOrganizationsCommand command, CancellationToken cancellationToken)
            {
                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var userId = _authenticatedUser.UserId;

                    // Check if domain is being changed
                    var isDomainChange = !string.IsNullOrWhiteSpace(command.Domain);

                    await _permissionService.EnsureCanUpdateOrgAsync( userId, command.OrganizationId, isDomainChange);

                    var org = await _organizationsRepository.GetByIdAsync(command.OrganizationId)
                                    ?? throw new ApiException("Organization could not be found.");

                    // Only update fields that were provided
                    if (!string.IsNullOrWhiteSpace(command.Name))
                        org.Name = command.Name;

                    if (!string.IsNullOrWhiteSpace(command.Description))
                        org.Description = command.Description;

                    if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
                        org.PhoneNumber = command.PhoneNumber;

                    if (!string.IsNullOrWhiteSpace(command.Address))
                        org.Address = command.Address;

                    if (!string.IsNullOrWhiteSpace(command.City))
                        org.City = command.City;

                    if (!string.IsNullOrWhiteSpace(command.Country))
                        org.Country = command.Country;

                    if (!string.IsNullOrWhiteSpace(command.Website))
                        org.Website = command.Website;

                    if (!string.IsNullOrWhiteSpace(command.LogoUrl))
                        org.LogoUrl = command.LogoUrl;

                    await _organizationsRepository.UpdateAsync(org);

                    ts.Complete();
                }

                return new Response<bool>(true, "Organization updated successfully.");
            }
        }
    }
}

