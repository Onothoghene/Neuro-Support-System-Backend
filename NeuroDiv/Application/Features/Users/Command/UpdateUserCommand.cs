using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Command
{
    public class UpdateUserCommand : IRequest<Response<bool>>
    {
        public required string FirstName { get; set; }
        public string? OtherName { get; set; }
        public required string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? GenderId { get; set; }

        public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Response<bool>>
        {
            private readonly IUserProfileRepositoryAsync _userProfileRepo;
            private readonly IMapper _mapper;
            private readonly IAuthenticatedUserService _user;
            private readonly IDateTimeService _dateTime;

            public UpdateUserCommandHandler(IUserProfileRepositoryAsync userProfileRepo, 
                                            IMapper mapper, IAuthenticatedUserService user)
            {
                _userProfileRepo = userProfileRepo;
                _mapper = mapper;
                _user = user;
            }

            public async Task<Response<bool>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
            {
                var userId = Guid.Parse(_user.UserId);

                var userProfile = await _userProfileRepo.GetUserByIdAsync(userId) ?? throw new ApiException("User profile not found.");

                userProfile.FirstName = !string.IsNullOrEmpty(command.FirstName) ? command.FirstName : userProfile.FirstName;
                userProfile.LastName = !string.IsNullOrEmpty(command.LastName) ? command.LastName : userProfile.LastName;
                userProfile.PhoneNumber = !string.IsNullOrWhiteSpace(command.PhoneNumber) ? command.PhoneNumber : userProfile.PhoneNumber;
                userProfile.GenderId = command.GenderId;

                userProfile.OtherName = command.OtherName;

                await _userProfileRepo.UpdateAsync(userProfile);

                return new Response<bool>(true, "Personal details updated successfully");
            }
        }
    }
}
