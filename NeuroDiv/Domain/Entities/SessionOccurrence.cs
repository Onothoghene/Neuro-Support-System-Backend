using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class SessionOccurrence : AuditableBaseEntity
    {
        public SessionOccurrence()
        {
            ChildSessionRecords = new HashSet<ChildSessionRecord>();
        }

        public Guid SessionClassId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

        /// <summary>When the session actually started.</summary>
        public DateTime? ActualStartTime { get; set; }

        /// <summary>When the session actually ended.</summary>
        public DateTime? ActualEndTime { get; set; }

        public string? GeneralNotes { get; set; }

        // Navigation
        public SessionClass SessionClass { get; set; }
        public SessionCancellation? Cancellation { get; set; }
        public SessionNoShow? NoShow { get; set; }
        public ICollection<ChildSessionRecord> ChildSessionRecords { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
