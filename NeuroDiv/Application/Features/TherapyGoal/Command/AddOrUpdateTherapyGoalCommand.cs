using Application.DTOs.TherapyGoal;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.TherapyGoal.Command
{
    public class AddOrUpdateTherapyGoalCommand : IRequest<Response<bool>>
    {
        public Guid? Id { get; set; }               // null = create, value = update
        public Guid ChildProfileId { get; set; }
        public Guid GoalCategoryId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? TargetDate { get; set; }
        public GoalStatus? Status { get; set; }
        public string? Notes { get; set; }

        public class AddOrUpdateTherapyGoalCommandHandler(ITherapyGoalRepositoryAsync goalRepository,
                                                          IAuthenticatedUserService authenticatedUser,
                                                          IMapper mapper)
            : IRequestHandler<AddOrUpdateTherapyGoalCommand, Response<bool>>
        {
            private readonly ITherapyGoalRepositoryAsync _goalRepository = goalRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
            private readonly IMapper _mapper = mapper;

            public async Task<Response<bool>> Handle(AddOrUpdateTherapyGoalCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (command.Id.HasValue)
                {
                    // ── Update 
                    var goal = await _goalRepository.GetByIdAsync(command.Id.Value)
                        ?? throw new ApiException("Therapy goal could not be found.");

                    if (!string.IsNullOrWhiteSpace(command.Title))
                        goal.Title = command.Title;

                    if (command.GoalCategoryId != Guid.Empty)
                        goal.GoalCategoryId = command.GoalCategoryId;

                    if (!string.IsNullOrWhiteSpace(command.Description))
                        goal.Description = command.Description;

                    if (command.TargetDate.HasValue)
                        goal.TargetDate = command.TargetDate;

                    if (command.Status.HasValue)
                        goal.Status = command.Status.Value;

                    goal.Notes = command.Notes;

                    await _goalRepository.UpdateAsync(goal);

                    ts.Complete();
                    return new Response<bool>(true, "Therapy goal updated successfully.");
                }
                else
                {
                    // ── Create
                    var goal = _mapper.Map<Domain.Entities.TherapyGoal>(new AddTherapyGoalRequest
                    {
                        GoalCategoryId = command.GoalCategoryId,
                        Title = command.Title,
                        Description = command.Description,
                        TargetDate = command.TargetDate,
                        Notes = command.Notes,
                    });

                    goal.ChildProfileId = command.ChildProfileId;
                    goal.Status = GoalStatus.NotStarted;

                    await _goalRepository.AddAsync(goal);

                    ts.Complete();
                    return new Response<bool>(true, "Therapy goal added successfully.");
                }
            }
        }
    }
}