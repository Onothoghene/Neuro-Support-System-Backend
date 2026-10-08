using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class AssessmentSection : AuditableBaseEntity
    {
        public AssessmentSection()
        {
            Questions = new HashSet<AssessmentQuestion>();
        }

        public Guid AssessmentTemplateId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        /// <summary>Controls display order of sections.</summary>
        public int DisplayOrder { get; set; } = 1;

        // Navigation
        public AssessmentTemplate AssessmentTemplate { get; set; }
        public ICollection<AssessmentQuestion> Questions { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
