using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/users"), Authorize(Roles = AppRoles.Admin)]
public class UsersController(UserManager<ApplicationUser> users, ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accounts = await users.Users.OrderBy(x => x.UserName).ToListAsync();
        var roles = new Dictionary<int, string>();
        foreach (var account in accounts)
            roles[account.Id] = (await users.GetRolesAsync(account)).FirstOrDefault() ?? "";
        var members = await db.Members.Include(x => x.Club).OrderBy(x => x.FullName).ToListAsync();
        return Ok(new UserListResponse(accounts, members, roles));
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (model.Role is not (AppRoles.Admin or AppRoles.ClubLeader or AppRoles.Member)) return BadRequest(new ApiError("Vai trò không hợp lệ."));
        var user = new ApplicationUser { UserName = model.Username, Email = model.Email, EmailConfirmed = true };
        var result = await users.CreateAsync(user, model.Password);
        if (!result.Succeeded) return BadRequest(new ApiError(string.Join(" ", result.Errors.Select(e => e.Description))));
        await users.AddToRoleAsync(user, model.Role);
        return NoContent();
    }

    [HttpPost("{id:int}/role")]
    public async Task<IActionResult> SetRole(int id, SetRoleRequest request)
    {
        var role = request.Role; var memberId = request.MemberId;
        if (role is not (AppRoles.Admin or AppRoles.ClubLeader or AppRoles.Member)) return BadRequest(new ApiError("Vai trò không hợp lệ."));
        if (await users.FindByIdAsync(id.ToString()) == null) return NotFound();
        try
        {
            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                var account = await users.FindByIdAsync(id.ToString()) ?? throw new InvalidOperationException("Tài khoản không còn tồn tại.");
                var linked = await db.Members.SingleOrDefaultAsync(x => x.UserId == id);
                if (memberId != null && linked != null && linked.Id != memberId)
                    throw new InvalidOperationException("Tài khoản đã liên kết hồ sơ khác. Hãy sửa liên kết tại Quản lý thành viên.");
                if (memberId != null && linked == null)
                {
                    linked = await db.Members.SingleOrDefaultAsync(x => x.Id == memberId);
                    if (linked == null || linked.UserId != null)
                        throw new InvalidOperationException("Hồ sơ không tồn tại hoặc đã có tài khoản.");
                    linked.UserId = id;
                }
                if (role == AppRoles.ClubLeader && (linked == null || linked.MemberStatus != DomainStatus.Active))
                    throw new InvalidOperationException("Chọn hồ sơ thành viên đang hoạt động để xác định CLB quản lý.");
                await db.SaveChangesAsync();
                var oldRoles = await users.GetRolesAsync(account);
                var removed = await users.RemoveFromRolesAsync(account, oldRoles);
                if (!removed.Succeeded) throw new InvalidOperationException(string.Join(" ", removed.Errors.Select(x => x.Description)));
                var added = await users.AddToRoleAsync(account, role);
                if (!added.Succeeded) throw new InvalidOperationException(string.Join(" ", added.Errors.Select(x => x.Description)));
                var updated = await users.UpdateSecurityStampAsync(account);
                if (!updated.Succeeded) throw new InvalidOperationException(string.Join(" ", updated.Errors.Select(x => x.Description)));
                await transaction.CommitAsync();
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError("Không thể đổi vai trò: " + ex.Message));
        }
        return Ok(new SetRoleResponse(users.GetUserId(User) == id.ToString()));
    }

    [HttpPost("{id:int}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var current = await users.GetUserAsync(User);
        if (current?.Id == id) return BadRequest(new ApiError("Không thể tự khóa tài khoản đang đăng nhập."));
        var user = await users.FindByIdAsync(id.ToString()); if (user == null) return NotFound();
        user.Status = user.Status == DomainStatus.Active ? "INACTIVE" : DomainStatus.Active; await users.UpdateAsync(user); return NoContent();
    }
}
