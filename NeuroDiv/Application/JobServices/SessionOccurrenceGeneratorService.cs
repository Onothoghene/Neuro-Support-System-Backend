using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.JobServices
{
    public class SessionOccurrenceGeneratorService(ISessionClassRepositoryAsync sessionClassRepository,
                                                   ISessionOccurrenceRepositoryAsync occurrenceRepository,
                                                   ILogger<SessionOccurrenceGeneratorService> logger) 
           : ISessionOccurrenceGeneratorService
    {
        private readonly ISessionClassRepositoryAsync _sessionClassRepository = sessionClassRepository;
        private readonly ISessionOccurrenceRepositoryAsync _occurrenceRepository = occurrenceRepository;
        private readonly ILogger<SessionOccurrenceGeneratorService> _logger = logger;

        private const int LookAheadDays = 30;

        public async Task GenerateUpcomingOccurrencesAsync()
        {
            _logger.LogInformation("Session occurrence generator started at {Time}", DateTime.UtcNow);

            var activeClasses = await _sessionClassRepository.GetAllActiveRecurringAsync();

            var lookAheadDate = DateTime.UtcNow.Date.AddDays(LookAheadDays);
            var generated = 0;

            foreach (var sessionClass in activeClasses)
            {
                try
                {
                    generated += await GenerateForClassAsync(sessionClass, lookAheadDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate occurrences for SessionClass {Id}", sessionClass.Id);
                }
            }

            _logger.LogInformation("Generator completed. {Count} occurrences generated.", generated);
        }

        private async Task<int> GenerateForClassAsync(SessionClass sessionClass, DateTime lookAheadDate)
        {
            var rule = sessionClass.RecurrenceRule;
            if (rule == null) return 0;

            // Deactivate class if recurrence period is over
            if (rule.RecurrenceEndDate.HasValue && rule.RecurrenceEndDate.Value < DateTime.UtcNow.Date)
            {
                sessionClass.IsActive = false;
                sessionClass.LastModified = DateTime.UtcNow;
                await _sessionClassRepository.UpdateAsync(sessionClass);
                return 0;
            }

            var daysOfWeek = rule.GetDaysOfWeek();
            var generated = 0;

            var latestOccurrence = await _occurrenceRepository.GetLatestByClassIdAsync(sessionClass.Id);

            var startDate = latestOccurrence != null ? latestOccurrence.ScheduledDate.AddDays(1)
                                                     : DateTime.UtcNow.Date;

            var current = startDate;

            while (current <= lookAheadDate)
            {
                if (daysOfWeek.Contains(current.DayOfWeek))
                {
                    if (rule.RecurrenceEndDate.HasValue && current > rule.RecurrenceEndDate.Value)
                        break;

                    var exists = await _occurrenceRepository.ExistsForClassAndDateAsync(sessionClass.Id, current);

                    if (!exists)
                    {
                        await _occurrenceRepository.AddAsync(new SessionOccurrence
                        {
                            SessionClassId = sessionClass.Id,
                            ScheduledDate = current,
                            StartTime = rule.StartTime,
                            EndTime = rule.EndTime,
                            Status = SessionStatus.Scheduled,
                        });
                        generated++;
                    }
                }

                current = current.AddDays(1);
            }

            return generated;
        }
    }

}
