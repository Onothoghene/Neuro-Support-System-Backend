using Domain.Common;
using System;

namespace Domain.Entities
{
    public class AssessmentQuestionOption : AuditableBaseEntity
    {
        public Guid AssessmentQuestionId { get; set; }
        public string OptionText { get; set; }
        public decimal Score { get; set; } = 0;
        public int DisplayOrder { get; set; } = 1;

        // Navigation
        public AssessmentQuestion AssessmentQuestion { get; set; }

        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
