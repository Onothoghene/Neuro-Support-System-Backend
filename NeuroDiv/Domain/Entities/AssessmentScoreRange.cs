using Domain.Common;
using System;

namespace Domain.Entities
{
    public class AssessmentScoreRange : AuditableBaseEntity
    {
        public Guid AssessmentTemplateId { get; set; }
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }

        /// <summary>e.g. "Non-Autistic", "Mild to Moderate", "Severe"</summary>
        public string Label { get; set; }

        /// <summary>
        /// Optional clinical description of what this range means.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Color code for UI display e.g. "#22c55e" (green = low concern)
        /// </summary>
        public string? ColorCode { get; set; }

        // Navigation
        public AssessmentTemplate AssessmentTemplate { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
