using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class SessionClass : AuditableBaseEntity
    {
        public SessionClass()
        {
            Occurrences = new HashSet<SessionOccurrence>();
        }

        public string Title { get; set; }
        public string? Description { get; set; }

        //Ownership
        public Guid ChildProfileId { get; set; }
        public Guid? OrganizationId { get; set; }

        //Mode
        public SessionMode Mode { get; set; } = SessionMode.Physical;

        //Recurrence
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// true  = generating occurrences normally
        /// false = stopped, no more occurrences generated
        /// </summary>
        public bool IsActive { get; set; } = true;

        //Navigation
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
        public ChildProfile ChildProfile { get; set; }
        public Organizations? Organization { get; set; }
        public SessionRecurrenceRule? RecurrenceRule { get; set; }
        public SessionOnlineDetails? OnlineDetails { get; set; }
        public ICollection<SessionOccurrence> Occurrences { get; set; }
    }
}
