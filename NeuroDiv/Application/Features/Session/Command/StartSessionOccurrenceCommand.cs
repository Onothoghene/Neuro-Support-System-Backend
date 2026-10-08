using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.Session.Command
{
    public class StartSessionOccurrenceCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }

        public class StartSessionOccurrenceCommandHandler(ISessionOccurrenceRepositoryAsync occurrenceRepository,
                                                          IAuthenticatedUserService authenticatedUser) 
              : IRequestHandler<StartSessionOccurrenceCommand, Response<bool>>
        {
            private readonly ISessionOccurrenceRepositoryAsync _occurrenceRepository = occurrenceRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;

            public async Task<Response<bool>> Handle(StartSessionOccurrenceCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var occurrence = await _occurrenceRepository.GetByIdAsync(command.Id) ??
                                 throw new ApiException("Session occurrence could not be found.");

                if (occurrence.Status != SessionStatus.Scheduled) 
                    throw new ApiException("Only scheduled session can be started.");

                occurrence.Status = SessionStatus.InProgress;
                occurrence.ActualStartTime = DateTime.UtcNow;

                await _occurrenceRepository.UpdateAsync(occurrence);

                ts.Complete();

                return new Response<bool>(true, "Session started.");
            }
        }
    }
}
