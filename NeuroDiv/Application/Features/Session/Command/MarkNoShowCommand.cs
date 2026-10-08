using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.Session.Command
{
    public class MarkNoShowCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public NoShowType NoShowType { get; set; }
        public string? Notes { get; set; }

        public class MarkNoShowCommandHandler(ISessionOccurrenceRepositoryAsync occurrenceRepository,
                                              IAuthenticatedUserService authenticatedUser,
                                              ISessionNoShowRepositoryAsync noShowRepository)
              : IRequestHandler<MarkNoShowCommand, Response<bool>>
        {
            private readonly ISessionOccurrenceRepositoryAsync _occurrenceRepository = occurrenceRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly ISessionNoShowRepositoryAsync _noShowRepository = noShowRepository;

            public async Task<Response<bool>> Handle(MarkNoShowCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var occurrence = await _occurrenceRepository.GetByIdWithDetailsAsync(command.Id) ??
                                 throw new ApiException("Session occurrence could not be found.");

                if (occurrence.Status == SessionStatus.Completed)
                    throw new ApiException("Completed sessions cannot be marked as no-show.");

                if (occurrence.Status == SessionStatus.Cancelled)
                    throw new ApiException("Cancelled sessions cannot be marked as no-show.");

                occurrence.Status = SessionStatus.NoShow;

                await _occurrenceRepository.UpdateAsync(occurrence);

                await _noShowRepository.AddAsync(new SessionNoShow
                {
                    SessionOccurrenceId = occurrence.Id,
                    NoShowType = command.NoShowType,
                    Notes = command.Notes,
                });

                ts.Complete();

                return new Response<bool>(true, "Session marked as no-show.");
            }
        }
    }
}
