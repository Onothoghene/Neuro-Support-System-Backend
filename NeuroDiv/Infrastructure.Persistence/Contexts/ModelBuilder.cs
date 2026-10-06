using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Contexts
{
    public partial class ApplicationDbContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // Universal filtering

            //Fluent Navigations

            //UserProfile
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.OtherName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.GenderId).HasMaxLength(10);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //TherapistProfile
            modelBuilder.Entity<TherapistProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Bio).HasMaxLength(1500);
                entity.Property(e => e.LicenseNumber).HasMaxLength(100);
                entity.Property(e => e.LicenseType).HasMaxLength(50);

                // One UserProfile → one optional TherapistProfile
                entity.HasOne(e => e.UserProfile)
                      .WithOne(u => u.TherapistProfile)
                      .HasForeignKey<TherapistProfile>(e => e.UserProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //TherapistSpecialization
            modelBuilder.Entity<TherapistSpecialization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Condition).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(e => e.TherapistProfile)
                      .WithMany(t => t.Specializations)
                      .HasForeignKey(e => e.TherapistProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //EmailChangeRequest
            modelBuilder.Entity<EmailChangeRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NewEmail).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.UserProfile)
                      .WithMany(u => u.EmailChangeRequest)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //Organizations
            modelBuilder.Entity<Organizations>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Domain).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.LogoUrl).HasMaxLength(500);

                // Unique domain — two orgs can't claim the same email domain
                //entity.HasIndex(e => e.Domain)
                //    .IsUnique()
                //    .HasFilter("[Domain] IS NOT NULL");

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //OrganizationUsers 
            modelBuilder.Entity<OrganizationUsers>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organizations)
                      .WithMany(o => o.OrganizationUsers)
                      .HasForeignKey(e => e.OrganizationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //OrganizationRoles
            modelBuilder.Entity<OrganizationRoles>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Organizations)
                      .WithMany(o => o.OrganizationRoles)
                      .HasForeignKey(e => e.OrganizationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //OrganizationUserRoles
            modelBuilder.Entity<OrganizationUserRoles>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.OrganizationUserRoles)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organizations)
                      .WithMany(o => o.OrganizationUserRoles)
                      .HasForeignKey(e => e.OrganizationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OrganizationRoles)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(e => e.OrganizationRoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // OrganizationUsersInvite
            modelBuilder.Entity<OrganizationUsersInvite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);

                // Token must be globally unique
                entity.HasIndex(e => e.Token).IsUnique();

                // Index for fast lookup by email + org
                entity.HasIndex(e => new { e.OrganizationId, e.Email });

                entity.HasOne(e => e.Organization)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //DiagnosisType
            modelBuilder.Entity<DiagnosisType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //GoalCategory
            modelBuilder.Entity<GoalCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //ChildProfile
            modelBuilder.Entity<ChildProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OtherName).HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
                entity.Property(e => e.DiagnosedBy).HasMaxLength(200);
                entity.Property(e => e.MedicalHistory).HasMaxLength(2000);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(100);

                entity.HasOne(e => e.DiagnosisType)
                      .WithMany(d => d.ChildProfiles)
                      .HasForeignKey(e => e.DiagnosisTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organization)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //TherapyGoal
            modelBuilder.Entity<TherapyGoal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(300);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(1500);

                entity.HasOne(e => e.ChildProfile)
                      .WithMany(c => c.TherapyGoals)
                      .HasForeignKey(e => e.ChildProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.GoalCategory)
                      .WithMany(g => g.TherapyGoals)
                      .HasForeignKey(e => e.GoalCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //ChildTherapistAssignment
            modelBuilder.Entity<ChildTherapistAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Ignore computed property — not stored in DB
                entity.Ignore(e => e.IsActive);

                entity.HasOne(e => e.ChildProfile)
                      .WithMany(c => c.TherapistAssignments)
                      .HasForeignKey(e => e.ChildProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Therapist)
                      .WithMany()
                      .HasForeignKey(e => e.TherapistId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //ParentProfile
            modelBuilder.Entity<ParentProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OtherName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Relationship).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.UserProfile)
                      .WithMany()
                      .HasForeignKey(e => e.UserProfileId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //ChildParent (many-to-many join)
            modelBuilder.Entity<ChildParent>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Prevent duplicate parent-child links
                entity.HasIndex(e => new { e.ChildProfileId, e.ParentProfileId }).IsUnique();

                entity.HasOne(e => e.ChildProfile)
                      .WithMany(c => c.Parents)
                      .HasForeignKey(e => e.ChildProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ParentProfile)
                      .WithMany(p => p.Children)
                      .HasForeignKey(e => e.ParentProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //SessionDuration
            modelBuilder.Entity<SessionDuration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Label).IsRequired().HasMaxLength(50);
            });

            //SessionClass
            modelBuilder.Entity<SessionClass>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.HasOne(e => e.Therapist)
                      .WithMany()
                      .HasForeignKey(e => e.TherapistId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ChildProfile)
                      .WithMany()
                      .HasForeignKey(e => e.ChildProfileId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organization)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //SessionRecurrenceRule
            modelBuilder.Entity<SessionRecurrenceRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DaysOfWeek).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.SessionClass)
                      .WithOne(s => s.RecurrenceRule)
                      .HasForeignKey<SessionRecurrenceRule>(e => e.SessionClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SessionDuration)
                      .WithMany()
                      .HasForeignKey(e => e.SessionDurationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //SessionOnlineDetails
            modelBuilder.Entity<SessionOnlineDetails>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MeetingLink).HasMaxLength(500);
                entity.Property(e => e.MeetingId).HasMaxLength(200);
                entity.Property(e => e.MeetingPassword).HasMaxLength(100);
                entity.Property(e => e.JoiningInstructions).HasMaxLength(1000);

                entity.HasOne(e => e.SessionClass)
                      .WithOne(s => s.OnlineDetails)
                      .HasForeignKey<SessionOnlineDetails>(e => e.SessionClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //SessionOccurrence
            modelBuilder.Entity<SessionOccurrence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GeneralNotes);

                entity.HasOne(e => e.SessionClass)
                      .WithMany(s => s.Occurrences)
                      .HasForeignKey(e => e.SessionClassId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //SessionCancellation
            modelBuilder.Entity<SessionCancellation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(1500);
                entity.Property(e => e.CancelledBy).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.SessionOccurrence)
                      .WithOne(o => o.Cancellation)
                      .HasForeignKey<SessionCancellation>(e => e.SessionOccurrenceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //SessionNoShow
            modelBuilder.Entity<SessionNoShow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(1500);

                entity.HasOne(e => e.SessionOccurrence)
                      .WithOne(o => o.NoShow)
                      .HasForeignKey<SessionNoShow>(e => e.SessionOccurrenceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //ChildSessionRecord
            modelBuilder.Entity<ChildSessionRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GeneralNotes).HasMaxLength(2000);

                // Prevent duplicate records for the same child in the same occurrence
                entity.HasIndex(e => new { e.SessionOccurrenceId, e.ChildProfileId }).IsUnique();

                entity.HasOne(e => e.SessionOccurrence)
                      .WithMany(o => o.ChildSessionRecords)
                      .HasForeignKey(e => e.SessionOccurrenceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ChildProfile)
                      .WithMany()
                      .HasForeignKey(e => e.ChildProfileId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //GoalProgressLog
            modelBuilder.Entity<GoalProgressLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProgressNote).HasMaxLength(1000);

                entity.HasOne(e => e.ChildSessionRecord)
                      .WithMany(r => r.GoalProgressLogs)
                      .HasForeignKey(e => e.ChildSessionRecordId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TherapyGoal)
                      .WithMany()
                      .HasForeignKey(e => e.TherapyGoalId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            ////AssessmentTemplate
            modelBuilder.Entity<AssessmentTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.TargetCondition).HasMaxLength(100);

                entity.HasOne(e => e.Organization)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByNavigation)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //AssessmentSection
            modelBuilder.Entity<AssessmentSection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.AssessmentTemplate)
                      .WithMany(t => t.Sections)
                      .HasForeignKey(e => e.AssessmentTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //AssessmentQuestion
            modelBuilder.Entity<AssessmentQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
                entity.Property(e => e.HelpText).HasMaxLength(500);

                entity.HasOne(e => e.AssessmentSection)
                      .WithMany(s => s.Questions)
                      .HasForeignKey(e => e.AssessmentSectionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //AssessmentQuestionOption
            modelBuilder.Entity<AssessmentQuestionOption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OptionText).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.AssessmentQuestion)
                      .WithMany(q => q.Options)
                      .HasForeignKey(e => e.AssessmentQuestionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //AssessmentScoreRange
            modelBuilder.Entity<AssessmentScoreRange>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ColorCode).HasMaxLength(20);

                entity.HasOne(e => e.AssessmentTemplate)
                      .WithMany(t => t.ScoreRanges)
                      .HasForeignKey(e => e.AssessmentTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //AssessmentSnapshot
            modelBuilder.Entity<AssessmentSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ScoreLabel).HasMaxLength(100);
                entity.Property(e => e.ClinicalNotes).HasMaxLength(2000);
                entity.Property(e => e.UploadedFilePath).HasMaxLength(500);
                entity.Property(e => e.VerifiedBy).HasMaxLength(200);

                entity.HasOne(e => e.AssessmentTemplate)
                      .WithMany()
                      .HasForeignKey(e => e.AssessmentTemplateId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ChildProfile)
                      .WithMany()
                      .HasForeignKey(e => e.ChildProfileId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Therapist)
                      .WithMany()
                      .HasForeignKey(e => e.TherapistId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SessionOccurrence)
                      .WithMany()
                      .HasForeignKey(e => e.SessionOccurrenceId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            //AssessmentResponse
            modelBuilder.Entity<AssessmentResponse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TextResponse).HasMaxLength(2000);

                entity.HasOne(e => e.AssessmentSnapshot)
                      .WithMany(s => s.Responses)
                      .HasForeignKey(e => e.AssessmentSnapshotId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AssessmentQuestion)
                      .WithMany()
                      .HasForeignKey(e => e.AssessmentQuestionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SelectedOption)
                      .WithMany()
                      .HasForeignKey(e => e.SelectedOptionId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

        }
    }
}
