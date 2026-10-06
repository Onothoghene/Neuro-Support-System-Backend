using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class AssessmentSnapshotRepositoryAsync : GenericRepositoryAsync<AssessmentSnapshot>, IAssessmentSnapshotRepositoryAsync
    {
        private readonly DbSet<AssessmentSnapshot> _AssessmentSnapshot;

        public AssessmentSnapshotRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _AssessmentSnapshot = dbContext.Set<AssessmentSnapshot>();
        }

        public async Task<List<AssessmentSnapshot>> GetAllByChildIdAsync(Guid childProfileId, Guid? templateId, 
                                                                         DateTime? fromDate, DateTime? toDate)
        {
            var query = _AssessmentSnapshot.Include(s => s.AssessmentTemplate)
                                           .Include(s => s.Therapist)
                                           .Where(s => s.ChildProfileId == childProfileId && !s.IsDeleted)
                                           .AsQueryable();

            if (templateId.HasValue)
                query = query.Where(s => s.AssessmentTemplateId == templateId.Value);

            if (fromDate.HasValue)
                query = query.Where(s => s.AssessmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.AssessmentDate <= toDate.Value);

            return await query.OrderByDescending(s => s.AssessmentDate).ToListAsync();
        }

        public async Task<AssessmentSnapshot?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _AssessmentSnapshot.Include(s => s.AssessmentTemplate)
                                            .Include(s => s.ChildProfile)
                                            .Include(s => s.Therapist)
                                            .Include(s => s.SessionOccurrence)
                                            .Include(s => s.Responses)
                                            .ThenInclude(r => r.AssessmentQuestion)
                                            .Include(s => s.Responses)
                                            .ThenInclude(r => r.SelectedOption)
                                            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<List<AssessmentSnapshot>> GetPendingVerificationAsync(Guid? organizationId)
        {
            // Returns legacy uploads that haven't been verified yet
            var query = _AssessmentSnapshot.Include(s => s.AssessmentTemplate)
                                           .Include(s => s.ChildProfile)
                                           .Include(s => s.Therapist)
                                           .Where(s => s.IsLegacyUpload && !s.IsVerified && !s.IsDeleted)
                                           .AsQueryable();

            return await query.OrderBy(s => s.AssessmentDate).ToListAsync();
        }
    }
}
