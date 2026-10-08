using Domain.Common;
using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class TherapyGoal : AuditableBaseEntity
    {
        public Guid ChildProfileId { get; set; }
        public Guid GoalCategoryId { get; set; }

        // ── Structured
        public string? Title { get; set; }
        public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
        public DateTime? TargetDate { get; set; }

        // ── Free text
        public string? Description { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public ChildProfile ChildProfile { get; set; }
        public GoalCategory GoalCategory { get; set; }

        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }

}
