using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class AssessmentQuestionRepositoryAsync : GenericRepositoryAsync<AssessmentQuestion>, IAssessmentQuestionRepositoryAsync
    {
        private readonly DbSet<AssessmentQuestion> _AssessmentQuestion;

        public AssessmentQuestionRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _AssessmentQuestion = dbContext.Set<AssessmentQuestion>();
        }

      
    }
}
