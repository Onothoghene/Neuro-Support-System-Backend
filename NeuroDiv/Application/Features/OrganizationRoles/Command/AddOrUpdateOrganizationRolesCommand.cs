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

namespace Application.Features.OrganizationRoles.Command
{
    public class AddOrUpdateOrganizationRolesCommand : IRequest<Response<bool>>
    {
        public Guid? Id { get; set; }              // null = create, has value = update
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public class AddOrUpdateOrganizationRolesCommandHandler : IRequestHandler<AddOrUpdateOrganizationRolesCommand, Response<bool>>
        {
            private readonly IMapper _mapper;
            private readonly IAuthenticatedUserService _authenticatedUser;
            private readonly IOrganizationRolesRepositoryAsync _orgRolesRepository;
            private readonly IOrganizationPermissionService _permissionService;

            public AddOrUpdateOrganizationRolesCommandHandler(IMapper mapper, IAuthenticatedUserService authenticatedUser,
                                                             IOrganizationRolesRepositoryAsync orgRolesRepository,
                                                             IOrganizationPermissionService permissionService)
            {
                _mapper = mapper;
                _authenticatedUser = authenticatedUser;
                _orgRolesRepository = orgRolesRepository;
                _permissionService = permissionService;
            }

            public async Task<Response<bool>> Handle(AddOrUpdateOrganizationRolesCommand command, CancellationToken cancellationToken)
            {
                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var userId = _authenticatedUser.UserId;

                    await _permissionService.EnsureCanManageRolesAsync( _authenticatedUser.UserId, command.OrganizationId);

                    //Update functionality
                    if (command.Id.HasValue && command.Id != Guid.Empty)
                    {
                        var role = await _orgRolesRepository.GetByIdAsync(command.Id.Value)
                                   ?? throw new ApiException("Role could not be found.");

                        // Default roles can have their description updated
                        // but their name is locked — it's referenced in code
                        if (role.IsDefault && role.Name != command.Name)
                            throw new ApiException("The name of a default role cannot be changed.");

                        role.Name = command.Name;
                        role.Description = command.Description;

                        await _orgRolesRepository.UpdateAsync(role);

                        ts.Complete();
                        return new Response<bool>(true, "Role updated successfully.");
                    }
                    else //Create Functionality
                    {
                        // Check name isn't already used in this org
                        var existingRole = await _orgRolesRepository.GetByNameAndOrgAsync(command.Name, command.OrganizationId)
                                           ?? throw new ApiException($"A role named '{command.Name}' already exists in this organization.");

                        var data = _mapper.Map<Domain.Entities.OrganizationRoles>(command);
                        await _orgRolesRepository.AddAsync(data);

                        ts.Complete();

                        return new Response<bool>(true, "Custom role created successfully.");
                    }                    
                }

            }
        }
    }
}

