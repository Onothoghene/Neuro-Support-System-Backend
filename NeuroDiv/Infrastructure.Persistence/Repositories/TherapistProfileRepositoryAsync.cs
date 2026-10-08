using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class TherapistProfileRepositoryAsync : GenericRepositoryAsync<TherapistProfile>, ITherapistProfileRepositoryAsync
    {
        private readonly DbSet<TherapistProfile> _therapistProfiles;

        public TherapistProfileRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _therapistProfiles = dbContext.Set<TherapistProfile>();
        }

        public async Task<TherapistProfile?> GetByUserIdAsync(Guid userProfileId)
        {
            return await _therapistProfiles.Include(t => t.CreatedByNavigation)
                                           .Include(t => t.Specializations)
                                           .FirstOrDefaultAsync(t => t.CreatedBy == userProfileId && !t.IsDeleted);
        }

        public async Task<TherapistProfile?> GetByIdAsync(Guid id)
        {
            return await _therapistProfiles.Include(t => t.CreatedByNavigation)
                                           .Include(t => t.Specializations)
                                           .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }
    }
}
