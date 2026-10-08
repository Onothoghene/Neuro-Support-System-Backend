using Application.DTOs.Session;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.Session.Command
{
    public class CreateSessionClassCommand : IRequest<Response<bool>>
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public Guid ChildProfileId { get; set; }
        public Guid? OrganizationId { get; set; }
        public SessionMode Mode { get; set; } = SessionMode.Physical;
        public bool IsRecurring { get; set; } = false;
        public SessionScheduleRequest Schedule { get; set; }
        public RecurrenceRequest? Recurrence { get; set; }
        public OnlineSessionDetailsRequest? OnlineDetails { get; set; }

        public class CreateSessionClassCommandHandler(ISessionClassRepositoryAsync sessionClassRepository,
                                                      ISessionOccurrenceRepositoryAsync occurrenceRepository,
                                                      ISessionOccurrenceGeneratorService generatorService,
                                                      IAuthenticatedUserService authenticatedUser,
                                                      IMapper mapper)
              : IRequestHandler<CreateSessionClassCommand, Response<bool>>
        {
            private readonly ISessionClassRepositoryAsync _sessionClassRepository = sessionClassRepository;
            private readonly ISessionOccurrenceRepositoryAsync _occurrenceRepository = occurrenceRepository;
            private readonly ISessionOccurrenceGeneratorService _generatorService = generatorService;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<bool>> Handle(CreateSessionClassCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                // Validations
                if (command.Mode != SessionMode.Physical && command.OnlineDetails == null)
                    throw new ApiException(
                        "Online details are required for online or hybrid sessions.");

                if (command.IsRecurring)
                {
                    if (command.Recurrence == null)
                        throw new ApiException(
                            "Recurrence settings are required for recurring sessions.");

                    if (!command.Recurrence.DaysOfWeek.Any())
                        throw new ApiException(
                            "At least one day of the week is required for recurring sessions.");
                }

                var therapistId = Guid.Parse(_authenticatedUser.UserId);

                var sessionClass = _mapper.Map<SessionClass>(command);
                sessionClass.IsActive = true;
                sessionClass.CreatedBy = therapistId;

                // Add recurrence rule
                if (command.IsRecurring && command.Recurrence != null)
                {
                    var rule = new SessionRecurrenceRule
                    {
                        StartTime = command.Schedule.StartTime,
                        EndTime = command.Schedule.EndTime,
                        SessionDurationId = command.Schedule.SessionDurationId,
                        RecurrenceEndDate = command.Recurrence.RecurrenceEndDate,
                    };
                    rule.SetDaysOfWeek(command.Recurrence.DaysOfWeek);
                    sessionClass.RecurrenceRule = rule;
                }

                // Add online details
                if (command.Mode != SessionMode.Physical && command.OnlineDetails != null)
                {
                    var sessionMode = _mapper.Map<SessionOnlineDetails>(command.OnlineDetails);

                    sessionClass.OnlineDetails = sessionMode;
                }

                var result = await _sessionClassRepository.AddAsync(sessionClass);

                if (!command.IsRecurring)
                {
                    // One-off session — create the single occurrence immediately
                    var occurrence = _mapper.Map<SessionOccurrence>(command.Schedule);
                    occurrence.SessionClassId = result.Id;

                    await _occurrenceRepository.AddAsync(occurrence);
                }
                else
                {
                    // Recurring — trigger immediate generation so therapist
                    // sees upcoming sessions right away (Hangfire maintains daily)
                    await _generatorService.GenerateUpcomingOccurrencesAsync();
                }

                ts.Complete();

                return new Response<bool>(true, "Session created successfully.");
            }
        }
    }
}
