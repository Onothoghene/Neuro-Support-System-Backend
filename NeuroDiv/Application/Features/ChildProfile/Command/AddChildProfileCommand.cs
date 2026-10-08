using Application.DTOs.ParentProfile;
using Application.DTOs.TherapyGoal;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.ChildProfile.Command
{
    public class AddChildProfileCommand : IRequest<Response<Guid>>
    {
        public required string FirstName { get; set; }
        public string? OtherName { get; set; }
        public required string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Guid DiagnosisTypeId { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public string? DiagnosedBy { get; set; }
        public string? MedicalHistory { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public Guid? OrganizationId { get; set; }
        public List<AddTherapyGoalRequest> TherapyGoals { get; set; } = new();
        public List<AddParentProfileRequest> Parents { get; set; } = new();

        public class AddChildProfileCommandHandler(IMapper mapper, IAuthenticatedUserService authenticatedUser,
                                             IChildProfileRepositoryAsync childProfileRepository,
                                             IParentProfileRepositoryAsync parentProfileRepository) : IRequestHandler<AddChildProfileCommand, Response<Guid>>
        {
            private readonly IChildProfileRepositoryAsync _childProfileRepository = childProfileRepository;
            private readonly IParentProfileRepositoryAsync _parentProfileRepository = parentProfileRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<Guid>> Handle(AddChildProfileCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var therapistId = Guid.Parse(_authenticatedUser.UserId);

                var childProfile = _mapper.Map<Domain.Entities.ChildProfile>(command);
                childProfile.CreatedBy = therapistId;

                // Map initial therapy goals
                childProfile.TherapyGoals = _mapper.Map<List<Domain.Entities.TherapyGoal>>(command.TherapyGoals);

                // Assign the creating therapist as Primary by default
                childProfile.TherapistAssignments =
                [
                    new()
                    {
                        TherapistId = therapistId,
                        Role        = AssignmentRole.Primary,
                        StartDate   = DateTime.UtcNow,
                    }
                ];

                var result = await _childProfileRepository.AddAsync(childProfile);

                // Add initial parents if provided
                foreach (var parentRequest in command.Parents)
                {
                    var parent = _mapper.Map<Domain.Entities.ParentProfile>(parentRequest);

                    var savedParent = await _parentProfileRepository.AddAsync(parent);

                    await _parentProfileRepository.AddChildParentAsync(new ChildParent
                    {
                        ChildProfileId = result.Id,
                        ParentProfileId = savedParent.Id,
                    });
                }

                ts.Complete();

                return new Response<Guid>(result.Id, "Child profile created successfully.");
            }
        }
    }
}

