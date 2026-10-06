using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class AssessmentScoreRangeRepositoryAsync : GenericRepositoryAsync<AssessmentScoreRange>, IAssessmentScoreRangeRepositoryAsync
    {
        private readonly DbSet<AssessmentScoreRange> _AssessmentScoreRange;

        public AssessmentScoreRangeRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _AssessmentScoreRange = dbContext.Set<AssessmentScoreRange>();
        }

      
    }
}
