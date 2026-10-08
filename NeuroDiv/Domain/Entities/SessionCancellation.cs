using Domain.Common;
using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class SessionCancellation : AuditableBaseEntity
    {
        public Guid SessionOccurrenceId { get; set; }
        public CancellationReason Reason { get; set; }
        public string? Notes { get; set; }
        public DateTime CancelledAt { get; set; }
        public string CancelledBy { get; set; }

        // Navigation
        public SessionOccurrence SessionOccurrence { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
