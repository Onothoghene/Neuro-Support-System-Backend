using Application.DTOs.Email;
using Application.DTOs.File;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.TherapistProfile.Command
{
    public class AddOrUpdateTherapistProfileCommand : IRequest<Response<bool>>
    {
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseType { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public bool IsAvailableForNewClients { get; set; } = true;
        public bool IsPublicProfile { get; set; } = false;
        public List<TherapistSpecializationRequest> Specializations { get; set; } = new();

        public class AddOrUpdateTherapistProfileCommandHandler : IRequestHandler<AddOrUpdateTherapistProfileCommand, Response<bool>>
        {
            private readonly IMapper _mapper;
            private readonly IAuthenticatedUserService _authenticatedUser;
            private readonly ITherapistProfileRepositoryAsync _therapistProfileRepository;

            public AddOrUpdateTherapistProfileCommandHandler(IMapper mapper, IAuthenticatedUserService authenticatedUser,
                                                            ITherapistProfileRepositoryAsync therapistProfileRepository)
            {
                _mapper = mapper;
                _authenticatedUser = authenticatedUser;
                _therapistProfileRepository = therapistProfileRepository;
            }

            public async Task<Response<bool>> Handle(AddOrUpdateTherapistProfileCommand command, CancellationToken cancellationToken)
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var userProfileId = Guid.Parse(_authenticatedUser.UserId);

                    var existingProfile = await _therapistProfileRepository.GetByUserIdAsync(userProfileId);

                    if (existingProfile != null)
                    {
                        // ── Update 
                        existingProfile.Bio = command.Bio;
                        existingProfile.YearsOfExperience = command.YearsOfExperience;
                        existingProfile.LicenseNumber = command.LicenseNumber;
                        existingProfile.LicenseType = command.LicenseType;
                        existingProfile.LicenseExpiryDate = command.LicenseExpiryDate;
                        existingProfile.IsPublicProfile = command.IsPublicProfile;

                        // Replace specializations entirely
                        existingProfile.Specializations.Clear();
                        existingProfile.Specializations = _mapper.Map<List<TherapistSpecialization>>(command.Specializations)
                                                                .Select(s =>{
                                                                    s.TherapistProfileId = existingProfile.Id;
                                                                    return s;
                                                                }).ToList();

                        await _therapistProfileRepository.UpdateAsync(existingProfile);

                        ts.Complete();
                        return new Response<bool>(true, "Therapist profile updated successfully.");
                    }
                    else
                    {
                        // ── Create 
                        var therapistProfile = _mapper.Map<Domain.Entities.TherapistProfile>(command);
                        therapistProfile.CreatedBy = userProfileId;

                        //therapistProfile.Specializations = _mapper.Map<List<TherapistSpecialization>>(command.Specializations)
                        //                                        .Select(s =>
                        //                                        {
                        //                                            s.CreatedBy = _authenticatedUser.UserId;
                        //                                            s.Created = DateTime.UtcNow;
                        //                                            return s;
                        //                                        }).ToList();

                        await _therapistProfileRepository.AddAsync(therapistProfile);

                        ts.Complete();
                        return new Response<bool>(true, "Therapist profile created successfully.");
                    }
                }
            }
        }
    }
}
