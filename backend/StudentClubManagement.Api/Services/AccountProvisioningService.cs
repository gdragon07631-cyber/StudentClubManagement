using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;

namespace StudentClubManagement.Services;

public class AccountProvisioningService(ApplicationDbContext db, UserManager<ApplicationUser> users)
{
    public async Task IssueAsync(int applicationId, string username, string password)
    {
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var application = await db.MembershipApplications.SingleOrDefaultAsync(x => x.Id == applicationId);
            if (application == null || application.Status != DomainStatus.Approved)
                throw new InvalidOperationException("Chỉ cấp tài khoản sau khi phê duyệt.");
            var member = await db.Members.SingleOrDefaultAsync(x => x.ClubId == application.ClubId && x.StudentCode == application.StudentCode);
            if (member == null) throw new InvalidOperationException("Không tìm thấy hồ sơ thành viên.");
            if (member.UserId != null) throw new InvalidOperationException("Thành viên đã được cấp tài khoản.");
            var user = new ApplicationUser { UserName = username.Trim(), Email = member.Email };
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded) throw new InvalidOperationException(string.Join(" ", created.Errors.Select(x => x.Description)));
            var assigned = await users.AddToRoleAsync(user, AppRoles.Member);
            if (!assigned.Succeeded) throw new InvalidOperationException(string.Join(" ", assigned.Errors.Select(x => x.Description)));
            member.UserId = user.Id;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        });
    }
}
