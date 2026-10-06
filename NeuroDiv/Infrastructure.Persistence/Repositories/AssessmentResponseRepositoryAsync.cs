using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class AssessmentResponseRepositoryAsync : GenericRepositoryAsync<AssessmentResponse>, IAssessmentResponseRepositoryAsync
    {
        private readonly DbSet<AssessmentResponse> _AssessmentResponse;

        public AssessmentResponseRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _AssessmentResponse = dbContext.Set<AssessmentResponse>();
        }

      
    }
}
