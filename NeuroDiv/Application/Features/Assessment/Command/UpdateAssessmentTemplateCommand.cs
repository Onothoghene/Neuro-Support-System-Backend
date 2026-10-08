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

namespace Application.Features.Assessment.Command
{
    public class UpdateAssessmentTemplateCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? TargetCondition { get; set; }
        public TemplateVisibility? Visibility { get; set; }
        public bool? IsActive { get; set; }

        public class UpdateAssessmentTemplateCommandHandler(IAssessmentTemplateRepositoryAsync assessmentTemplateRepository,
                                                            IAuthenticatedUserService authenticatedUser)
              : IRequestHandler<UpdateAssessmentTemplateCommand, Response<bool>>
        {
            private readonly IAssessmentTemplateRepositoryAsync _assessmentTemplateRepository = assessmentTemplateRepository;
            private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;

            public async Task<Response<bool>> Handle(UpdateAssessmentTemplateCommand command, CancellationToken cancellationToken)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var template = await _assessmentTemplateRepository.GetByIdWithDetailsAsync(command.Id) ?? 
                               throw new ApiException("Assessment template could not be found.");

                // System templates cannot be modified
                if (template.IsSystemTemplate)
                    throw new ApiException("System-provided assessment templates cannot be modified.");

                if (!string.IsNullOrWhiteSpace(command.Name))
                    template.Name = command.Name;

                template.Description = command.Description;
                template.TargetCondition = command.TargetCondition;

                if (command.Visibility.HasValue)
                    template.Visibility = command.Visibility.Value;

                if (command.IsActive.HasValue)
                    template.IsActive = command.IsActive.Value;

                template.LastModified = DateTime.UtcNow;
                template.LastModifiedBy = Guid.Parse(_authenticatedUser.UserId);

                await _assessmentTemplateRepository.UpdateAsync(template);

                ts.Complete();

                return new Response<bool>(true, "Assessment template updated successfully.");
            }
        }
    }
}