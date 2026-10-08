using Domain.Common;
using System;

namespace Domain.Entities
{
    public class OrganizationUsersInvite : AuditableBaseEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; } // The target email
        public string Token { get; set; } // Unique link token
        public bool IsAccepted { get; set; } = false;
        public DateTime ExpiryDate { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid OrganizationRoleId { get; set; }
        public Guid? AcceptedByUserId { get; set; }

        public Organizations Organization { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
