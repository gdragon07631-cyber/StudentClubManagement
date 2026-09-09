using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentClubManagement.Models;

namespace StudentClubManagement.Data;

public static class DemoSeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var legacyRole = await roles.FindByNameAsync("DEPARTMENT_LEADER");
        if (legacyRole != null && !await roles.RoleExistsAsync(AppRoles.ClubLeader))
        {
            legacyRole.Name = AppRoles.ClubLeader;
            var renamed = await roles.UpdateAsync(legacyRole);
            if (!renamed.Succeeded) throw new InvalidOperationException("Không thể cập nhật vai trò trưởng CLB.");
        }
        foreach (var role in new[] { AppRoles.Admin, AppRoles.ClubLeader, AppRoles.Member })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole<int>(role) { NormalizedName = role });
        if (await db.Clubs.AnyAsync())
        {
            const string secondClubName = "CLB Kỹ năng Sinh viên";
            if (!await db.Clubs.AnyAsync(x => x.ClubName == secondClubName))
            {
                var extraClub = new Club { ClubName = secondClubName, Description = "Câu lạc bộ thứ hai dùng để minh họa phạm vi quản lý theo ClubId." };
                db.Clubs.Add(extraClub); await db.SaveChangesAsync();
                db.Members.Add(new Member { ClubId = extraClub.Id, StudentCode = "SV002", FullName = "Thành viên CLB Kỹ năng", Email = "member2@club.demo", ClassName = "QTKD-K21", Course = "K21", Position = "Thành viên" });
                db.Activities.Add(new Activity { ClubId = extraClub.Id, ActivityName = "Workshop kỹ năng thuyết trình", Description = "Dữ liệu mẫu của câu lạc bộ thứ hai", StartTime = DateTime.Today.AddDays(10).AddHours(8), EndTime = DateTime.Today.AddDays(10).AddHours(11), Location = "Phòng học A2", Status = "PLANNED" });
                await db.SaveChangesAsync();
            }
            return;
        }

        var club = new Club { ClubName = "CLB Công nghệ Sinh viên", Description = "Dữ liệu mẫu phục vụ trình diễn đồ án." };
        var secondClub = new Club { ClubName = "CLB Kỹ năng Sinh viên", Description = "Câu lạc bộ thứ hai dùng để minh họa phạm vi quản lý theo ClubId." };
        db.Clubs.AddRange(club, secondClub); await db.SaveChangesAsync();
        var media = new Department { ClubId = club.Id, DepartmentName = "Ban Truyền thông", Description = "Phụ trách truyền thông." };
        var events = new Department { ClubId = club.Id, DepartmentName = "Ban Sự kiện", Description = "Phụ trách tổ chức hoạt động." };
        db.Departments.AddRange(media, events); await db.SaveChangesAsync();

        var admin = await CreateUser(users, "admin", "admin@club.demo", "Admin@123", AppRoles.Admin);
        var leader = await CreateUser(users, "leader01", "leader@club.demo", "Leader@123", AppRoles.ClubLeader);
        var memberUser = await CreateUser(users, "SV001", "member@club.demo", "Member@123", AppRoles.Member);
        var leaderMember = new Member { UserId = leader.Id, ClubId = club.Id, DepartmentId = events.Id, StudentCode = "SVL001", FullName = "Trưởng câu lạc bộ Demo", Email = leader.Email!, ClassName = "CNTT-K20", Course = "K20", Position = "Trưởng câu lạc bộ" };
        var member = new Member { UserId = memberUser.Id, ClubId = club.Id, DepartmentId = events.Id, StudentCode = "SV001", FullName = "Thành viên Demo", Email = memberUser.Email!, ClassName = "CNTT-K21", Course = "K21", Position = "Thành viên" };
        var secondClubMember = new Member { ClubId = secondClub.Id, StudentCode = "SV002", FullName = "Thành viên CLB Kỹ năng", Email = "member2@club.demo", ClassName = "QTKD-K21", Course = "K21", Position = "Thành viên" };
        db.Members.AddRange(leaderMember, member, secondClubMember); await db.SaveChangesAsync(); media.LeaderMemberId = null; events.LeaderMemberId = leaderMember.Id;
        db.MemberSkills.AddRange(new MemberSkill { MemberId = member.Id, SkillName = "Thiết kế", SkillLevel = "Khá", Experience = "Poster sự kiện" }, new MemberSkill { MemberId = leaderMember.Id, SkillName = "Tổ chức", SkillLevel = "Tốt", Experience = "Điều phối hoạt động" });
        var activity = new Activity { ClubId = club.Id, ActivityName = "Ngày hội chào tân sinh viên", Description = "Hoạt động demo", StartTime = DateTime.Today.AddDays(7).AddHours(8), EndTime = DateTime.Today.AddDays(7).AddHours(17), Location = "Hội trường", Status = "PLANNED" };
        var meeting = new Meeting { ClubId = club.Id, Title = "Họp chuẩn bị sự kiện", MeetingTime = DateTime.Today.AddDays(2).AddHours(19), Location = "Phòng CLB", Agenda = "Phân công và rà soát tiến độ" };
        var secondActivity = new Activity { ClubId = secondClub.Id, ActivityName = "Workshop kỹ năng thuyết trình", Description = "Dữ liệu mẫu của câu lạc bộ thứ hai", StartTime = DateTime.Today.AddDays(10).AddHours(8), EndTime = DateTime.Today.AddDays(10).AddHours(11), Location = "Phòng học A2", Status = "PLANNED" };
        db.AddRange(activity, meeting, secondActivity); await db.SaveChangesAsync();
        var task = new ClubTask { ActivityId = activity.Id, Title = "Thiết kế poster", Description = "Poster truyền thông", Priority = "HIGH", StartDate = DateTime.Today, DueDate = DateTime.Today.AddDays(5), Progress = 25, Status = "IN_PROGRESS" };
        db.Tasks.Add(task); await db.SaveChangesAsync(); db.TaskAssignments.Add(new() { TaskId = task.Id, MemberId = member.Id });
        db.ActivityRegistrations.Add(new() { ActivityId = activity.Id, MemberId = member.Id });
        db.ActivityAttendance.Add(new() { ActivityId = activity.Id, MemberId = leaderMember.Id, AttendanceStatus = "PRESENT" });
        db.MeetingAttendance.Add(new() { MeetingId = meeting.Id, MemberId = member.Id, AttendanceStatus = "PRESENT" });
        var rule = new PointRule { RuleName = "Điểm demo tham gia hoạt động", SourceType = "ACTIVITY", PointValue = 10, EffectiveFrom = DateTime.Today, Status = DomainStatus.Active };
        db.PointRules.Add(rule); await db.SaveChangesAsync(); db.PointHistory.Add(new() { MemberId = member.Id, PointRuleId = rule.Id, SourceType = "ACTIVITY", SourceId = activity.Id, Points = 10, Reason = "Dữ liệu demo - không phải công thức chính thức" });
        db.FinancialTransactions.AddRange(new FinancialTransaction { ClubId = club.Id, TransactionType = "INCOME", Category = "Quỹ", Amount = 5000000, Description = "Quỹ demo", TransactionDate = DateTime.Today, CreatedById = admin.Id }, new FinancialTransaction { ClubId = club.Id, TransactionType = "EXPENSE", Category = "Sự kiện", Amount = 750000, Description = "Vật dụng demo", TransactionDate = DateTime.Today, CreatedById = admin.Id });
        db.MembershipApplications.AddRange(new MembershipApplication { ClubId = club.Id, StudentCode = "SVNEW01", FullName = "Sinh viên Đăng ký", Email = "applicant@demo.local", ClassName = "CNTT-K22", Course = "K22" }, new MembershipApplication { ClubId = club.Id, StudentCode = "SVNEW02", FullName = "Ứng viên Demo", Email = "applicant2@demo.local", Status = DomainStatus.Rejected, RejectionReason = "Dữ liệu minh họa" });
        db.Notifications.Add(new() { UserId = memberUser.Id, Title = "Nhiệm vụ mới", Content = "Bạn được giao thiết kế poster.", NotificationType = "TASK" });
        await db.SaveChangesAsync();
    }
    private static async Task<ApplicationUser> CreateUser(UserManager<ApplicationUser> manager, string username, string email, string password, string role)
    {
        var user = new ApplicationUser { UserName = username, Email = email, EmailConfirmed = true };
        var result = await manager.CreateAsync(user, password); if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        await manager.AddToRoleAsync(user, role); return user;
    }
}
