using Domain.Common;
using System;

namespace Domain.Entities
{
    public class OrganizationUsers : AuditableBaseEntity
    {
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
        public bool IsActive { get; set; }
       // public Guid OrganizationRoleId { get; set; }
        public DateTime JoinedAt { get; set; }

        public UserProfile User { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
        public Organizations Organizations { get; set; }
    }
}
