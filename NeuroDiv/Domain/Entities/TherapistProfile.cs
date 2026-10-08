using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class TherapistProfile : AuditableBaseEntity
    {
        public TherapistProfile()
        {
            Specializations = new HashSet<TherapistSpecialization>();
        }

        // ── Professional info
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LicenseNumber { get; set; }

        /// <summary>
        /// e.g. "BCBA", "SLP", "OT", "Psychologist"
        /// </summary>
        public string? LicenseType { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }

        // ── Find a Therapist (future) 
        /// <summary>
        /// Controls whether this therapist appears in public search.
        /// Only relevant for freelancers when Find a Therapist is built.
        /// </summary>
        public bool IsPublicProfile { get; set; } = false;

        public UserProfile CreatedByNavigation { get; set; }
        public ICollection<TherapistSpecialization> Specializations { get; set; }

    }
}
