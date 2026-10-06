using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class AssessmentQuestionOptionRepositoryAsync : GenericRepositoryAsync<AssessmentQuestionOption>, IAssessmentQuestionOptionRepositoryAsync
    {
        private readonly DbSet<AssessmentQuestionOption> _AssessmentQuestionOption;

        public AssessmentQuestionOptionRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _AssessmentQuestionOption = dbContext.Set<AssessmentQuestionOption>();
        }

      
    }
}
