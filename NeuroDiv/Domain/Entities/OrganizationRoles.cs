using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class OrganizationRoles : AuditableBaseEntity
    {
        public OrganizationRoles()
        {
            UserRoles = new HashSet<OrganizationUserRoles>();
        }

        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid? OrganizationId { get; set; }
        public bool IsDefault { get; set; }

        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
        public Organizations? Organizations { get; set; }
        public ICollection<OrganizationUserRoles> UserRoles { get; set; }
    }
}
