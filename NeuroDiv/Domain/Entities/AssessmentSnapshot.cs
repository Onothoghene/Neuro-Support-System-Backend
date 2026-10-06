using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class AssessmentSnapshot : AuditableBaseEntity
    {
        public AssessmentSnapshot()
        {
            Responses = new HashSet<AssessmentResponse>();
        }

        public Guid AssessmentTemplateId { get; set; }
        public Guid ChildProfileId { get; set; }
        public Guid TherapistId { get; set; }

        /// <summary>Null if standalone assessment.</summary>
        public Guid? SessionOccurrenceId { get; set; }

        public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;

        //Scoring
        public decimal TotalScore { get; set; }

        /// <summary>
        /// The matched score range label e.g. "Mild to Moderate Autism"
        /// Null for text-only assessments with no scoring.
        /// </summary>
        public string? ScoreLabel { get; set; }

        public string? ClinicalNotes { get; set; }

        //Legacy Upload
        public bool IsLegacyUpload { get; set; } = false;
        public string? UploadedFilePath { get; set; }

        /// <summary>
        /// For legacy uploads:
        /// false = AI extracted, awaiting human verification
        /// true  = therapist has reviewed and confirmed the data
        /// Always true for digital assessments.
        /// </summary>
        public bool IsVerified { get; set; } = true;

        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }

        // Navigation
        public AssessmentTemplate AssessmentTemplate { get; set; }
        public ChildProfile ChildProfile { get; set; }
        public UserProfile Therapist { get; set; }
        public SessionOccurrence? SessionOccurrence { get; set; }
        public ICollection<AssessmentResponse> Responses { get; set; }
    }
}
