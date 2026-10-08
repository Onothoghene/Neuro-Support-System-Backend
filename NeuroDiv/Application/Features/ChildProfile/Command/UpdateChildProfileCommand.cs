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

namespace Application.Features.ChildProfile.Command
{
    public class UpdateChildProfileCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? OtherName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Guid? DiagnosisTypeId { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public string? DiagnosedBy { get; set; }
        public string? MedicalHistory { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }

        public class UpdateChildProfileCommandHandler : IRequestHandler<UpdateChildProfileCommand, Response<bool>>
        {
            private readonly IChildProfileRepositoryAsync _childProfileRepository;
            private readonly IAuthenticatedUserService _authenticatedUser;
            public UpdateChildProfileCommandHandler(IMapper mapper, IAuthenticatedUserService authenticatedUser,
                                                   IChildProfileRepositoryAsync childProfileRepository)
            {
                _childProfileRepository = childProfileRepository;
                _authenticatedUser = authenticatedUser;
            }

            public async Task<Response<bool>> Handle(UpdateChildProfileCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var child = await _childProfileRepository.GetByIdAsync(command.Id)
                            ?? throw new ApiException("Child profile could not be found.");

                if (!string.IsNullOrWhiteSpace(command.FirstName))
                    child.FirstName = command.FirstName;

                if (!string.IsNullOrWhiteSpace(command.LastName))
                    child.LastName = command.LastName;

                child.OtherName = command.OtherName;

                if (command.DateOfBirth.HasValue)
                    child.DateOfBirth = command.DateOfBirth.Value;

                if (!string.IsNullOrWhiteSpace(command.Gender))
                    child.Gender = command.Gender;

                if (!string.IsNullOrWhiteSpace(command.ProfilePictureUrl))
                    child.ProfilePictureUrl = command.ProfilePictureUrl;

                if (command.DiagnosisTypeId.HasValue)
                    child.DiagnosisTypeId = command.DiagnosisTypeId.Value;

                if (command.DiagnosisDate.HasValue)
                    child.DiagnosisDate = command.DiagnosisDate;

                if (!string.IsNullOrWhiteSpace(command.DiagnosedBy))
                    child.DiagnosedBy = command.DiagnosedBy;

                if (!string.IsNullOrWhiteSpace(command.MedicalHistory))
                    child.MedicalHistory = command.MedicalHistory;

                if (!string.IsNullOrWhiteSpace(command.EmergencyContactName))
                    child.EmergencyContactName = command.EmergencyContactName;

                if (!string.IsNullOrWhiteSpace(command.EmergencyContactPhone))
                    child.EmergencyContactPhone = command.EmergencyContactPhone;

                if (!string.IsNullOrWhiteSpace(command.EmergencyContactRelationship))
                    child.EmergencyContactRelationship = command.EmergencyContactRelationship;

                await _childProfileRepository.UpdateAsync(child);

                ts.Complete();

                return new Response<bool>(true, "Child profile updated successfully.");
            }
        }
    }
}

