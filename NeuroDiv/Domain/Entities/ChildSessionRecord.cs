using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class ChildSessionRecord : AuditableBaseEntity
    {
        public ChildSessionRecord()
        {
            GoalProgressLogs = new HashSet<GoalProgressLog>();
        }

        public Guid SessionOccurrenceId { get; set; }
        public Guid ChildProfileId { get; set; }
        public string? GeneralNotes { get; set; }
        public ChildEngagement? Engagement { get; set; }

        // Navigation
        public SessionOccurrence SessionOccurrence { get; set; }
        public ChildProfile ChildProfile { get; set; }
        public ICollection<GoalProgressLog> GoalProgressLogs { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
