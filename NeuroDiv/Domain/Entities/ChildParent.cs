using Domain.Common;
using System;

namespace Domain.Entities
{
    public class ChildParent : AuditableBaseEntity
    {
        public Guid ChildProfileId { get; set; }
        public Guid ParentProfileId { get; set; }

        // Navigation
        public ChildProfile ChildProfile { get; set; }
        public ParentProfile ParentProfile { get; set; }

        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}