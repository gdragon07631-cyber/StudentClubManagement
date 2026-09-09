using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class UsersController(UserManager<ApplicationUser> users, ApplicationDbContext db, SignInManager<ApplicationUser> signIn) : Controller
{
    public async Task<IActionResult> Index()
    {
        var accounts = await users.Users.OrderBy(x => x.UserName).ToListAsync();
        var roles = new Dictionary<int, string>();
        foreach (var account in accounts)
            roles[account.Id] = (await users.GetRolesAsync(account)).FirstOrDefault() ?? "";
        ViewBag.Members = await db.Members.Include(x => x.Club).OrderBy(x => x.FullName).ToListAsync();
        ViewBag.CurrentRoles = roles;
        return View(accounts);
    }
    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model) { if (model.Role is not (AppRoles.Admin or AppRoles.ClubLeader or AppRoles.Member)) ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ."); if (!ModelState.IsValid) return View(model); var user = new ApplicationUser { UserName = model.Username, Email = model.Email, EmailConfirmed = true }; var result = await users.CreateAsync(user, model.Password); if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return View(model); } await users.AddToRoleAsync(user, model.Role); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(int id, string role, int? memberId)
    {
        if (role is not (AppRoles.Admin or AppRoles.ClubLeader or AppRoles.Member)) return BadRequest();
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
            TempData["Error"] = "Không thể đổi vai trò: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
        TempData["Success"] = "Đã lưu vai trò. Người dùng cần đăng nhập lại để sử dụng quyền mới.";
        if (users.GetUserId(User) == id.ToString())
        {
            await signIn.RefreshSignInAsync((await users.FindByIdAsync(id.ToString()))!);
            if (role != AppRoles.Admin) return RedirectToAction("Index", "Dashboard");
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id) { var current = await users.GetUserAsync(User); if (current?.Id == id) { TempData["Error"] = "Không thể tự khóa tài khoản đang đăng nhập."; return RedirectToAction(nameof(Index)); } var user = await users.FindByIdAsync(id.ToString()); if (user == null) return NotFound(); user.Status = user.Status == DomainStatus.Active ? "INACTIVE" : DomainStatus.Active; await users.UpdateAsync(user); return RedirectToAction(nameof(Index)); }
}

