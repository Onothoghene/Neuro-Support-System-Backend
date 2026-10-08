using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class DiagnosisType : AuditableBaseEntity
    {
        public DiagnosisType()
        {
            ChildProfiles = new HashSet<ChildProfile>();
        }

        public required string Name { get; set; }        // e.g. "Autism Spectrum Disorder"
        public string? Code { get; set; }       // e.g. "ASD" — short code for reports
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<ChildProfile> ChildProfiles { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
