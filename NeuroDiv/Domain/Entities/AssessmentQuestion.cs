using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class AssessmentQuestion : AuditableBaseEntity
    {
        public AssessmentQuestion()
        {
            Options = new HashSet<AssessmentQuestionOption>();
        }

        public Guid AssessmentSectionId { get; set; }
        public string Text { get; set; }
        public string? HelpText { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool IsRequired { get; set; } = true;
        public int DisplayOrder { get; set; } = 1;

        // Navigation
        public AssessmentSection AssessmentSection { get; set; }
        public ICollection<AssessmentQuestionOption> Options { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
