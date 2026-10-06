using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Infrastructure.Persistence.Contexts
{
    public partial class ApplicationDbContext : DbContext
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<Organizations> Organizations { get; set; }
        public DbSet<OrganizationUsers> OrganizationUsers { get; set; }
        public DbSet<OrganizationUsersInvite> OrganizationUsersInvite { get; set; }
        public DbSet<OrganizationRoles> OrganizationRoles { get; set; }
        public DbSet<OrganizationUserRoles> OrganizationUserRoles { get; set; }
        public DbSet<EmailChangeRequest> EmailChangeRequest { get; set; }
        public DbSet<DiagnosisType> DiagnosisType { get; set; }
        public DbSet<ChildProfile> ChildProfile { get; set; }
        public DbSet<ParentProfile> ParentProfile { get; set; }
        public DbSet<ChildParent> ChildParent { get; set; }
        public DbSet<ChildTherapistAssignment> ChildTherapistAssignment { get; set; }
        public DbSet<GoalCategory> GoalCategory { get; set; }
        public DbSet<TherapyGoal> TherapyGoal { get; set; }
        public DbSet<SessionDuration> SessionDuration { get; set; }
        public DbSet<SessionClass> SessionClass { get; set; }
        public DbSet<ChildSessionRecord> ChildSessionRecord { get; set; }
        public DbSet<GoalProgressLog> GoalProgressLog { get; set; }
        public DbSet<SessionCancellation> SessionCancellation { get; set; }
        public DbSet<SessionNoShow> SessionNoShow { get; set; }
        public DbSet<SessionRecurrenceRule> SessionRecurrenceRule { get; set; }
        public DbSet<SessionOccurrence> SessionOccurrence { get; set; }
        public DbSet<SessionOnlineDetails> SessionOnlineDetails { get; set; }
        public DbSet<TherapistProfile> TherapistProfile { get; set; }
        public DbSet<TherapistSpecialization> TherapistSpecialization { get; set; }
        public DbSet<AssessmentTemplate> AssessmentTemplates { get; set; }
        public DbSet<AssessmentSection> AssessmentSections { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
        public DbSet<AssessmentQuestionOption> AssessmentQuestionOptions { get; set; }
        public DbSet<AssessmentScoreRange> AssessmentScoreRanges { get; set; }
        public DbSet<AssessmentSnapshot> AssessmentSnapshots { get; set; }
        public DbSet<AssessmentResponse> AssessmentResponses { get; set; }


        //public DbSet<Comments> Comments { get; set; }
        //public DbSet<FileTemp> FileTemp { get; set; }
        //public DbSet<Payment> Payment { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                string authUser;

                if (_authenticatedUser.UserId != null)
                {
                    authUser = _authenticatedUser.UserId.ToString();
                }
                else
                {
                    authUser = null;
                }

                if (entry.Entity.IsDeleted == true)
                {
                    entry.Entity.Deleted = _dateTime.NowUtc;
                    entry.Entity.DeletedBy = authUser;
                }
                else
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.Created = _dateTime.NowUtc;
                            entry.Entity.CreatedBy = authUser;
                            break;
                        case EntityState.Modified:
                            entry.Entity.LastModified = _dateTime.NowUtc;
                            entry.Entity.LastModifiedBy = authUser;
                            break;
                    }
                }

            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);

            base.OnModelCreating(modelBuilder);

           // modelBuilder.SeedRoles();

            //modelBuilder.SeedDepartments();

            //All Decimals will have 18,6 Range
            foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,6)");
            }
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
