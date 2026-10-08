using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    public class SessionRecurrenceRule : AuditableBaseEntity
    {
        public Guid SessionClassId { get; set; }

        /// <summary>
        /// Days stored as comma-separated string e.g. "Tuesday,Thursday"
        /// Use GetDaysOfWeek() and SetDaysOfWeek() helpers.
        /// </summary>
        public string DaysOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid? SessionDurationId { get; set; }

        /// <summary>Null = no end date, recurs indefinitely.</summary>
        public DateTime? RecurrenceEndDate { get; set; }

        // Navigation
        public SessionClass SessionClass { get; set; }
        public SessionDuration? SessionDuration { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }

        // Helpers
        public List<DayOfWeek> GetDaysOfWeek()
        {
            return DaysOfWeek.Split(',').Select(d => Enum.Parse<DayOfWeek>(d.Trim()))
                                        .ToList();
        }

        public void SetDaysOfWeek(List<DayOfWeek> days)
        {
            DaysOfWeek = string.Join(",", days.Select(d => d.ToString()));
        }
    }
}
