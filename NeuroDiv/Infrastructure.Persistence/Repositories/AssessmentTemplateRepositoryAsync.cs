using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class AssessmentTemplateRepositoryAsync : GenericRepositoryAsync<AssessmentTemplate>, IAssessmentTemplateRepositoryAsync
    {
        private readonly DbSet<AssessmentTemplate> _AssessmentTemplate;

        public AssessmentTemplateRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _AssessmentTemplate = dbContext.Set<AssessmentTemplate>();
        }

        public async Task<List<AssessmentTemplate>> GetAllAsync(Guid? organizationId, Guid? therapistId, 
                                                                bool? isSystemTemplate, bool? isActive, 
                                                                string? targetCondition)
        {
            var query = _AssessmentTemplate.Where(t => !t.IsDeleted)
                                           .AsQueryable();

            // System templates visible to all
            // Org-wide templates visible within the org
            // Private templates visible only to creator
            if (organizationId.HasValue && therapistId.HasValue)
            {
                query = query.Where(t => t.IsSystemTemplate ||
                                   (t.OrganizationId == organizationId.Value
                                   && t.Visibility == TemplateVisibility.OrgWide) ||
                                   (t.CreatedBy == therapistId.Value
                                   && t.Visibility == TemplateVisibility.Private));
            }

            if (isSystemTemplate.HasValue)
                query = query.Where(t => t.IsSystemTemplate == isSystemTemplate.Value);

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(targetCondition))
                query = query.Where(t => t.TargetCondition != null &&
                    t.TargetCondition.ToLower().Contains(targetCondition.ToLower()));

            return await query.OrderByDescending(t => t.IsSystemTemplate)
                              .ThenBy(t => t.Name)
                              .ToListAsync();
        }

        public async Task<AssessmentTemplate?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _AssessmentTemplate.Include(t => t.Sections
                                            .OrderBy(s => s.DisplayOrder))
                                            .ThenInclude(s => s.Questions.OrderBy(q => q.DisplayOrder))
                                            .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                                            .Include(t => t.ScoreRanges.OrderBy(r => r.MinScore))
                                            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }
    }
}
