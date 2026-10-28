using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Models;

namespace StudentClubManagement.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MembershipApplication> MembershipApplications => Set<MembershipApplication>();
    public DbSet<MemberSkill> MemberSkills => Set<MemberSkill>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityRegistration> ActivityRegistrations => Set<ActivityRegistration>();
    public DbSet<ActivityAttendance> ActivityAttendance => Set<ActivityAttendance>();
    public DbSet<ClubTask> Tasks => Set<ClubTask>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingAttendance> MeetingAttendance => Set<MeetingAttendance>();
    public DbSet<PointRule> PointRules => Set<PointRule>();
    public DbSet<PointHistory> PointHistory => Set<PointHistory>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AiLog> AiLogs => Set<AiLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ApplicationUser>().ToTable("users");
        b.Entity<IdentityRole<int>>().ToTable("roles");
        b.Entity<IdentityUserRole<int>>().ToTable("user_roles");
        b.Entity<IdentityUserClaim<int>>().ToTable("user_claims");
        b.Entity<IdentityUserLogin<int>>().ToTable("user_logins");
        b.Entity<IdentityRoleClaim<int>>().ToTable("role_claims");
        b.Entity<IdentityUserToken<int>>().ToTable("user_tokens");
        b.Entity<ApplicationUser>().HasIndex(x => x.Email).IsUnique();
        b.Entity<ApplicationUser>().HasOne(x => x.Member).WithOne(x => x.User)
            .HasForeignKey<Member>(x => x.UserId).OnDelete(DeleteBehavior.SetNull);

        b.Entity<Club>().ToTable("clubs");
        b.Entity<Department>().ToTable("departments");
        b.Entity<Member>().ToTable("members");
        b.Entity<MembershipApplication>().ToTable("membership_applications");
        b.Entity<MemberSkill>().ToTable("member_skills");
        b.Entity<Activity>().ToTable("activities", t => t.HasCheckConstraint("CK_activities_dates", "[EndTime] >= [StartTime]"));
        b.Entity<ActivityRegistration>().ToTable("activity_registrations");
        b.Entity<ActivityAttendance>().ToTable("activity_attendance");
        b.Entity<ClubTask>().ToTable("tasks", t => t.HasCheckConstraint("CK_tasks_progress", "[Progress] >= 0 AND [Progress] <= 100"));
        b.Entity<TaskAssignment>().ToTable("task_assignments");
        b.Entity<Meeting>().ToTable("meetings");
        b.Entity<MeetingAttendance>().ToTable("meeting_attendance");
        b.Entity<PointRule>().ToTable("point_rules");
        b.Entity<PointHistory>().ToTable("point_history");
        b.Entity<FinancialTransaction>().ToTable("financial_transactions", t => t.HasCheckConstraint("CK_financial_amount", "[Amount] > 0"));
        b.Entity<Notification>().ToTable("notifications");
        b.Entity<AiLog>().ToTable("ai_logs");

        // SQL Server forbids the cascade cycle Club -> Department -> Member -> Department.
        // The application already prevents deleting departments that still have members.
        b.Entity<Department>().HasOne(x => x.LeaderMember).WithMany().HasForeignKey(x => x.LeaderMemberId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Member>().HasOne(x => x.Department).WithMany(x => x.Members).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Member>().HasIndex(x => new { x.ClubId, x.StudentCode }).IsUnique();
        b.Entity<Member>().HasIndex(x => x.DepartmentId);
        b.Entity<Member>().HasIndex(x => x.MemberStatus);
        b.Entity<MemberSkill>().HasIndex(x => new { x.MemberId, x.SkillName }).IsUnique();
        b.Entity<Activity>().HasIndex(x => new { x.ClubId, x.StartTime });
        b.Entity<Activity>().HasIndex(x => x.Status);
        b.Entity<ActivityRegistration>().HasIndex(x => new { x.ActivityId, x.MemberId }).IsUnique();
        b.Entity<ActivityAttendance>().HasIndex(x => new { x.ActivityId, x.MemberId }).IsUnique();
        b.Entity<MeetingAttendance>().HasIndex(x => new { x.MeetingId, x.MemberId }).IsUnique();
        b.Entity<TaskAssignment>().HasIndex(x => new { x.TaskId, x.MemberId }).IsUnique();
        // Keep cascade delete on the activity/meeting side only. Both parents belong to
        // the same club, so cascading from Member as well creates multiple paths in SQL Server.
        b.Entity<ActivityRegistration>().HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ActivityAttendance>().HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<MeetingAttendance>().HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ClubTask>().HasIndex(x => new { x.ActivityId, x.Status });
        b.Entity<ClubTask>().HasIndex(x => x.DueDate);
        b.Entity<PointHistory>().HasIndex(x => new { x.MemberId, x.RecordedAt });
        b.Entity<FinancialTransaction>().HasIndex(x => x.TransactionDate);
        b.Entity<FinancialTransaction>().HasIndex(x => new { x.TransactionType, x.TransactionDate });
        b.Entity<Notification>().HasIndex(x => new { x.UserId, x.IsRead });
        b.Entity<FinancialTransaction>().Property(x => x.Amount).HasPrecision(18, 2);
        b.Entity<PointRule>().Property(x => x.PointValue).HasPrecision(10, 2);
        b.Entity<PointHistory>().Property(x => x.Points).HasPrecision(10, 2);
    }
}
