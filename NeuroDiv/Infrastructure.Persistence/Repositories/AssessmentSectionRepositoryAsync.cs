using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class AssessmentSectionRepositoryAsync : GenericRepositoryAsync<AssessmentSection>, IAssessmentSectionRepositoryAsync
    {
        private readonly DbSet<AssessmentSection> _AssessmentSection;

        public AssessmentSectionRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _AssessmentSection = dbContext.Set<AssessmentSection>();
        }

      
    }
}
