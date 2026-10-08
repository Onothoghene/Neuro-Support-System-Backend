using Domain.Common;
using System;

namespace Domain.Entities
{
    // ── Email change fields ───────────────────────────────────────────────
    /// <summary>
    /// Temporarily holds the new email until OTP is verified.
    /// Null when no email change is in progress.
    /// </summary>
    public class EmailChangeRequest : AuditableBaseEntity
    {
        public EmailChangeRequest()
        {
        }

        /// <summary>The email before the change — for reference/audit.</summary>
        public string CurrentEmail { get; set; }

        /// <summary>The new email the user wants to switch to.</summary>
        public string NewEmail { get; set; }

        /// <summary>OTP sent to the new email for verification.</summary>
        public int OtpCode { get; set; }

        /// <summary>When this request expires — 15 minutes from creation.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Whether this OTP has been used.
        /// Prevents replay attacks — an OTP can only be used once.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        // Navigation
        public UserProfile CreatedByNavigation { get; set; }
        public UserProfile LastModifiedByNavigation { get; set; }
        public UserProfile DeletedByNavigation { get; set; }
    }
}
