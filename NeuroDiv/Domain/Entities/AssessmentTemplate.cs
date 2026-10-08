using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class AssessmentTemplate : AuditableBaseEntity
    {
        public AssessmentTemplate()
        {
            Sections = new HashSet<AssessmentSection>();
            ScoreRanges = new HashSet<AssessmentScoreRange>();
        }

        public string Name { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// The condition this template is designed for.
        /// e.g. "Autism", "ADHD", "General"
        /// </summary>
        public string? TargetCondition { get; set; }

        /// <summary>
        /// true  = system-provided (CARS, Vanderbilt etc.) — read-only
        /// false = custom — created by org or therapist
        /// </summary>
        public bool IsSystemTemplate { get; set; } = false;

        public TemplateVisibility Visibility { get; set; } = TemplateVisibility.Private;

        /// <summary>Null for system templates.</summary>
        public Guid? OrganizationId { get; set; }

        public bool IsActive { get; set; } = true;

        //Scoring
        /// <summary>Minimum possible total score on this template.</summary>
        public decimal MinPossibleScore { get; set; } = 0;

        /// <summary>Maximum possible total score on this template.</summary>
        public decimal MaxPossibleScore { get; set; } = 0;

        /// <summary>
        /// Whether this template uses automatic score calculation.
        /// false = therapist enters overall impression manually.
        /// </summary>
        public bool HasAutoScoring { get; set; } = true;

        // Navigation
        public Organizations? Organization { get; set; }
        public UserProfile? CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
        public ICollection<AssessmentSection> Sections { get; set; }
        public ICollection<AssessmentScoreRange> ScoreRanges { get; set; }
    }
}
