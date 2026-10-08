using Domain.Common;
using System;

namespace Domain.Entities
{
    public class AssessmentResponse : AuditableBaseEntity
    {
        public Guid AssessmentSnapshotId { get; set; }
        public Guid AssessmentQuestionId { get; set; }

        /// <summary>For Scale/MultiChoice/YesNo questions.</summary>
        public Guid? SelectedOptionId { get; set; }

        /// <summary>For Text questions.</summary>
        public string? TextResponse { get; set; }

        /// <summary>Score from the selected option — 0 for text questions.</summary>
        public decimal Score { get; set; } = 0;

        // Navigation
        public AssessmentSnapshot AssessmentSnapshot { get; set; }
        public AssessmentQuestion AssessmentQuestion { get; set; }
        public AssessmentQuestionOption? SelectedOption { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
