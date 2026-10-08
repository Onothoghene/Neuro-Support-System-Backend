using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class EmailChangeRequestRepositoryAsync: GenericRepositoryAsync<EmailChangeRequest>, IEmailChangeRequestRepositoryAsync
    {
        private readonly DbSet<EmailChangeRequest> _emailChangeRequest;

        public EmailChangeRequestRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _emailChangeRequest = dbContext.Set<EmailChangeRequest>();
        }

        public async Task<EmailChangeRequest?> GetPendingByUserProfileIdAsync(Guid userId)
        {
            return await _emailChangeRequest.Where(e => e.CreatedBy == userId && !e.IsUsed 
                                                  && !e.IsDeleted && e.ExpiresAt > DateTime.UtcNow)
                                            .OrderByDescending(e => e.Created)
                                            .FirstOrDefaultAsync();
        }
    }
}
