using Domain.Common;
using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class GoalProgressLog : AuditableBaseEntity
    {
        public Guid ChildSessionRecordId { get; set; }
        public Guid TherapyGoalId { get; set; }

        /// <summary>Free text progress note for this goal this session.</summary>
        public string? ProgressNote { get; set; }

        /// <summary>1-5 rating of progress made on this goal this session.</summary>
        public int? ProgressRating { get; set; }

        /// <summary>
        /// Optional — if the therapist wants to update the goal status
        /// after this session. Null = no status change.
        /// </summary>
        public GoalStatus? StatusUpdate { get; set; }

        // Navigation
        public ChildSessionRecord ChildSessionRecord { get; set; }
        public TherapyGoal TherapyGoal { get; set; }
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }

    }
}
