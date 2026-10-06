using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IAssessmentSnapshotRepositoryAsync : IGenericRepositoryAsync<AssessmentSnapshot>
    {
        Task<AssessmentSnapshot?> GetByIdWithDetailsAsync(Guid id);
        Task<List<AssessmentSnapshot>> GetAllByChildIdAsync(Guid childProfileId, Guid? templateId,
                                                            DateTime? fromDate, DateTime? toDate);
        Task<List<AssessmentSnapshot>> GetPendingVerificationAsync(Guid? organizationId);
    }
}
