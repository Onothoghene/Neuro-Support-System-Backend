using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class ChildProfile : AuditableBaseEntity
    {
        public ChildProfile()
        {
            TherapyGoals = new HashSet<TherapyGoal>();
            TherapistAssignments = new HashSet<ChildTherapistAssignment>();
            Parents = new HashSet<ChildParent>();
        }

        // ── Basic Info
        public string FirstName { get; set; }
        public string? OtherName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // ── Clinical Info
        public Guid DiagnosisTypeId { get; set; }
        public DateTime? DiagnosisDate { get; set; }

        /// <summary>Name of doctor/specialist who made the diagnosis.</summary>
        public string? DiagnosedBy { get; set; }

        /// <summary>Additional clinical notes not captured elsewhere.</summary>
        public string? MedicalHistory { get; set; }

        // ── Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }

        // ── Ownership
        /// <summary>
        /// Null for freelancer-owned children.
        /// Populated for org-owned children.
        /// </summary>
        public Guid? OrganizationId { get; set; }

        public bool IsActive { get; set; } = true;

        // ── Navigation
        public DiagnosisType DiagnosisType { get; set; }
        public Organizations? Organization { get; set; }
        public ICollection<TherapyGoal> TherapyGoals { get; set; }
        public ICollection<ChildTherapistAssignment> TherapistAssignments { get; set; }
        public ICollection<ChildParent> Parents { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
