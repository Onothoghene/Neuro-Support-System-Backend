using Domain.Common;
using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class SessionOnlineDetails : AuditableBaseEntity
    {
        /// <summary>Belongs to SessionClass — not per occurrence.</summary>
        public Guid SessionClassId { get; set; }

        public OnlinePlatform Platform { get; set; }

        /// <summary>
        /// Manually pasted by therapist for now.
        /// Auto-generated when Integrations module is built.
        /// </summary>
        public string? MeetingLink { get; set; }

        public string? MeetingId { get; set; }
        public string? MeetingPassword { get; set; }
        public string? JoiningInstructions { get; set; }

        // Navigation
        public SessionClass SessionClass { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
