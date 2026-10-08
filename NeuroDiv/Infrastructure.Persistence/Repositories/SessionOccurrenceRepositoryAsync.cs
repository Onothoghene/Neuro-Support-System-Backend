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
    public class SessionOccurrenceRepositoryAsync : GenericRepositoryAsync<SessionOccurrence>, ISessionOccurrenceRepositoryAsync
    {
        private readonly DbSet<SessionOccurrence> _SessionOccurrence;

        public SessionOccurrenceRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _SessionOccurrence = dbContext.Set<SessionOccurrence>();
        }

        public async Task<bool> ExistsForClassAndDateAsync(Guid sessionClassId, DateTime date)
        {
            return await _SessionOccurrence.AnyAsync(o => o.SessionClassId == sessionClassId
                                                    && o.ScheduledDate.Date == date.Date
                                                    && !o.IsDeleted);
        }

        public async Task<List<SessionOccurrence>> GetAllAsync(Guid? sessionClassId, Guid? therapistId, Guid? childProfileId, SessionStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _SessionOccurrence.Include(o => o.SessionClass)
                                          .ThenInclude(s => s.CreatedByNavigation)
                                          .Include(o => o.SessionClass)
                                          .ThenInclude(s => s.ChildProfile)
                                          .Where(o => !o.IsDeleted)
                                          .AsQueryable();

            if (sessionClassId.HasValue)
                query = query.Where(o => o.SessionClassId == sessionClassId.Value);

            if (therapistId.HasValue)
                query = query.Where(o => o.SessionClass.CreatedBy == therapistId.Value);

            if (childProfileId.HasValue)
                query = query.Where(o => o.SessionClass.ChildProfileId == childProfileId.Value);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.ScheduledDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.ScheduledDate <= toDate.Value);

            return await query.OrderBy(o => o.ScheduledDate)
                              .ThenBy(o => o.StartTime)
                              .ToListAsync();
        }

        public async Task<SessionOccurrence?> GetByIdWithDetailsAsync(Guid id)
        {
            //return await _SessionOccurrence.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            return await _SessionOccurrence.Include(o => o.SessionClass)
                                           .ThenInclude(s => s.CreatedByNavigation)
                                           .Include(o => o.SessionClass)
                                           .ThenInclude(s => s.ChildProfile)
                                           .Include(o => o.SessionClass)
                                           .ThenInclude(s => s.OnlineDetails)
                                           .Include(o => o.Cancellation)
                                           .Include(o => o.NoShow)
                                           .Include(o => o.ChildSessionRecords)
                                           .ThenInclude(r => r.ChildProfile)
                                           .Include(o => o.ChildSessionRecords)
                                           .ThenInclude(r => r.GoalProgressLogs)
                                           .ThenInclude(g => g.TherapyGoal)
                                           .ThenInclude(t => t.GoalCategory)
                                           .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }
        
        public async Task<SessionOccurrence?> GetByIdAsync(Guid id)
        {
           return await _SessionOccurrence.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        }

        public async Task<SessionOccurrence?> GetLatestByClassIdAsync(Guid sessionClassId)
        {
            return await _SessionOccurrence.Where(o => o.SessionClassId == sessionClassId && !o.IsDeleted)
                                            .OrderByDescending(o => o.ScheduledDate)
                                            .FirstOrDefaultAsync();
        }
    }
}
