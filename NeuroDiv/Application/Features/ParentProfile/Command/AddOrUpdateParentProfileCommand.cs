using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.ParentProfile.Command
{
    public class AddOrUpdateParentProfileCommand : IRequest<Response<bool>>
    {
        public Guid? Id { get; set; }
        public Guid ChildProfileId { get; set; }
        public required string FirstName { get; set; }
        public string? OtherName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Relationship { get; set; }
        public bool IsPrimaryContact { get; set; } = false;

        public class AddOrUpdateParentProfileCommandHandler(IParentProfileRepositoryAsync parentProfileRepository,
                                                            IAuthenticatedUserService authenticatedUser, IMapper mapper)
               : IRequestHandler<AddOrUpdateParentProfileCommand, Response<bool>>
        {
            private readonly IParentProfileRepositoryAsync _parentProfileRepository = parentProfileRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<bool>> Handle(AddOrUpdateParentProfileCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (command.Id.HasValue)
                {
                    // ── Update 
                    var parent = await _parentProfileRepository.GetByIdAsync(command.Id.Value)
                        ?? throw new ApiException("Parent profile could not be found.");

                    if (!string.IsNullOrWhiteSpace(command.FirstName))
                        parent.FirstName = command.FirstName;

                    if (!string.IsNullOrWhiteSpace(command.LastName))
                        parent.LastName = command.LastName;

                    parent.OtherName = command.OtherName;
                    parent.Email = command.Email;
                    parent.PhoneNumber = command.PhoneNumber;
                    parent.Relationship = command.Relationship;
                    parent.IsPrimaryContact = command.IsPrimaryContact;

                    await _parentProfileRepository.UpdateAsync(parent);

                    ts.Complete();
                    return new Response<bool>(true, "Parent profile updated successfully.");
                }
                else
                {
                    // ── Create 
                    var parent = _mapper.Map<Domain.Entities.ParentProfile>(command);
                    //new AddParentProfileRequest
                    //{
                    //    FirstName = command.FirstName,
                    //    OtherName = command.OtherName,
                    //    LastName = command.LastName,
                    //    Email = command.Email,
                    //    PhoneNumber = command.PhoneNumber,
                    //    Relationship = command.Relationship,
                    //    IsPrimaryContact = command.IsPrimaryContact,
                    //});

                    var savedParent = await _parentProfileRepository.AddAsync(parent);

                    // Link to child
                    await _parentProfileRepository.AddChildParentAsync(new ChildParent
                    {
                        ChildProfileId = command.ChildProfileId,
                        ParentProfileId = savedParent.Id,
                    });

                    ts.Complete();
                    return new Response<bool>(true, "Parent profile added successfully.");
                }
            }
        }
    }
}