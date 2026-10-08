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
    public class SessionClassRepositoryAsync : GenericRepositoryAsync<SessionClass>, ISessionClassRepositoryAsync
    {
        private readonly DbSet<SessionClass> _SessionClass;

        public SessionClassRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _SessionClass = dbContext.Set<SessionClass>();
        }

        public async Task<SessionClass?> GetByIdWithDetailsAsync(Guid id)
        {
            //return await _SessionClass.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            return await _SessionClass.Include(s => s.CreatedByNavigation)
                                      .Include(s => s.ChildProfile)
                                      .Include(s => s.Organization)
                                      .Include(s => s.RecurrenceRule)
                                      .ThenInclude(r => r.SessionDuration)
                                      .Include(s => s.OnlineDetails)
                                      .Include(s => s.Occurrences
                                      .Where(o => o.ScheduledDate >= DateTime.UtcNow.Date)
                                      .OrderBy(o => o.ScheduledDate))
                                      .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<List<SessionClass>> GetAllActiveRecurringAsync()
        {
            return await _SessionClass.Where(s => s.IsActive && s.IsRecurring && s.RecurrenceRule != null
                                                          && !s.IsDeleted)
                                      .Include(s => s.RecurrenceRule)
                                      .ToListAsync();
        }

        public async Task<List<SessionClass>> GetAllAsync(Guid? organizationId, Guid? therapistId, Guid? childProfileId, bool? isActive)
        {
            var query = _SessionClass.Include(s => s.CreatedByNavigation)
                                     .Include(s => s.ChildProfile)
                                     .Include(s => s.RecurrenceRule)
                                     .Where(s => !s.IsDeleted)
                                     .AsQueryable();

            if (organizationId.HasValue)
                query = query.Where(s => s.OrganizationId == organizationId.Value);

            if (therapistId.HasValue)
                query = query.Where(s => s.CreatedBy == therapistId.Value);

            if (childProfileId.HasValue)
                query = query.Where(s => s.ChildProfileId == childProfileId.Value);

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            return await query.OrderByDescending(s => s.Created)
                              .ToListAsync();
        }

        //public async Task<List<SessionClass>> GetAllAsync(Guid? organizationId, Guid? therapistId, Guid? childProfileId,
        //                                            SessionStatus? status, SessionType? type,
        //                                            DateTime? fromDate, DateTime? toDate)
        //{
        //    var query = _session.Include(s => s.Therapist)
        //                         .Include(s => s.SessionDuration)
        //                         .Include(s => s.ChildSessionRecords)
        //                         .ThenInclude(r => r.ChildProfile)
        //                         .Where(s => !s.IsDeleted)
        //                         .AsQueryable();

        //    if (organizationId.HasValue)
        //        query = query.Where(s => s.OrganizationId == organizationId.Value);

        //    if (therapistId.HasValue)
        //        query = query.Where(s => s.TherapistId == therapistId.Value);

        //    if (childProfileId.HasValue)
        //        query = query.Where(s => s.ChildSessionRecords
        //                     .Any(r => r.ChildProfileId == childProfileId.Value));

        //    if (status.HasValue)
        //        query = query.Where(s => s.Status == status.Value);

        //    if (type.HasValue)
        //        query = query.Where(s => s.Type == type.Value);

        //    if (fromDate.HasValue)
        //        query = query.Where(s => s.ScheduledDate >= fromDate.Value);

        //    if (toDate.HasValue)
        //        query = query.Where(s => s.ScheduledDate <= toDate.Value);

        //    return await query.OrderBy(s => s.ScheduledDate)
        //                      .ThenBy(s => s.StartTime)
        //                      .ToListAsync();
        //}

    }
}
