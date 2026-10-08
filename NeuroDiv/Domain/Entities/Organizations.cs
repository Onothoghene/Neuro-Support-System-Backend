using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Organizations : AuditableBaseEntity
    {
        public Organizations()
        {
            OrganizationUsers = new HashSet<OrganizationUsers>();
            OrganizationRoles = new HashSet<OrganizationRoles>();
            OrganizationUserRoles = new HashSet<OrganizationUserRoles>();
            //Activities = new HashSet<Activity>();
        }

        public required string Name { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Domain { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }

        public ICollection<OrganizationUsers> OrganizationUsers { get; set; }
        public ICollection<OrganizationRoles> OrganizationRoles { get; set; } 
        public ICollection<OrganizationUserRoles> OrganizationUserRoles { get; set; } 
        //  public ICollection<Activity> Activities { get; set; }
    }
}
