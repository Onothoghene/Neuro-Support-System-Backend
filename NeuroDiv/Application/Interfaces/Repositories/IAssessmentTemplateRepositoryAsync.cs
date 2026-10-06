using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IAssessmentTemplateRepositoryAsync : IGenericRepositoryAsync<AssessmentTemplate>
    {
        Task<AssessmentTemplate?> GetByIdWithDetailsAsync(Guid id);
        Task<List<AssessmentTemplate>> GetAllAsync(Guid? organizationId, Guid? therapistId,
                                                   bool? isSystemTemplate, bool? isActive,
                                                   string? targetCondition);
    }
}
