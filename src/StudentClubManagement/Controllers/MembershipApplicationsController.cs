using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Authorization;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

public class MembershipApplicationsController(ApplicationDbContext db, IClubService service, UserManager<ApplicationUser> users, IAccessScopeService scope, AccountProvisioningService accounts) : Controller
{
    [AllowAnonymous, HttpGet] public async Task<IActionResult> Apply() { await LoadClubs(); return View(); }
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(MembershipApplicationViewModel vm)
    {
        if (!await db.Clubs.AnyAsync(x => x.Id == vm.ClubId)) ModelState.AddModelError(nameof(vm.ClubId), "Vui lòng chọn câu lạc bộ hợp lệ.");
        if (!ModelState.IsValid) { await LoadClubs(); return View(vm); }
        db.MembershipApplications.Add(new() { ClubId = vm.ClubId, StudentCode = vm.StudentCode, FullName = vm.FullName, Email = vm.Email, Phone = vm.Phone, ClassName = vm.ClassName, Course = vm.Course, BirthYear = vm.BirthYear, Interests = vm.Interests, JoinReason = vm.JoinReason, Expectations = vm.Expectations });
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Status), new { code = vm.StudentCode });
    }
    [AllowAnonymous] public async Task<IActionResult> Status(string code) => View(await db.MembershipApplications.Where(x => x.StudentCode == code).OrderByDescending(x => x.AppliedAt).FirstOrDefaultAsync());
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader)]
    public async Task<IActionResult> Index(string? q, int? clubId, string? status, string sort = "newest")
    {
        var query = db.MembershipApplications.Include(x => x.Club).AsQueryable();
        if (User.IsInRole(AppRoles.ClubLeader)) { var current = await scope.MemberAsync(User); if (current == null) return Forbid(); query = query.Where(x => x.ClubId == current.ClubId); }
        else if (clubId != null) query = query.Where(x => x.ClubId == clubId);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.FullName.Contains(q) || x.StudentCode.Contains(q) || x.Email.Contains(q));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = sort == "name" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.AppliedAt);
        ViewBag.Clubs = new SelectList(await db.Clubs.OrderBy(x => x.ClubName).ToListAsync(), "Id", "ClubName", clubId); ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(await query.ToListAsync());
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id) { if (!await CanManage(id)) return Forbid(); var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException(); await service.ApproveApplicationAsync(id, u.Id); TempData["Success"] = "Đã duyệt đơn đăng ký. Bạn có thể cấp tài khoản cho sinh viên."; return RedirectToAction(nameof(IssueAccount), new { id }); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason) { if (!await CanManage(id)) return Forbid(); if (string.IsNullOrWhiteSpace(reason)) { TempData["Error"] = "Cần nhập lý do từ chối."; return RedirectToAction(nameof(Index)); } var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException(); await service.RejectApplicationAsync(id, u.Id, reason); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet]
    public async Task<IActionResult> IssueAccount(int id)
    {
        if (!await CanManage(id)) return Forbid();
        var application = await db.MembershipApplications.FindAsync(id);
        if (application == null) return NotFound();
        if (application.Status != DomainStatus.Approved) return BadRequest("Chỉ cấp tài khoản sau khi phê duyệt.");
        var member = await db.Members.Include(x => x.User).SingleOrDefaultAsync(x => x.ClubId == application.ClubId && x.StudentCode == application.StudentCode);
        if (member == null) return NotFound();
        return View(new IssueAccountViewModel { Id = id, FullName = member.FullName, Username = member.StudentCode, ExistingUsername = member.User?.UserName });
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> IssueAccount(IssueAccountViewModel model)
    {
        if (!await CanManage(model.Id)) return Forbid();
        var application = await db.MembershipApplications.FindAsync(model.Id);
        if (application == null) return NotFound();
        if (application.Status != DomainStatus.Approved) return BadRequest("Chỉ cấp tài khoản sau khi phê duyệt.");
        var member = await db.Members.SingleOrDefaultAsync(x => x.ClubId == application.ClubId && x.StudentCode == application.StudentCode);
        if (member == null) return NotFound();
        model.FullName = member.FullName;
        model.ExistingUsername = null;
        if (member.UserId != null) return RedirectToAction(nameof(IssueAccount), new { id = model.Id });
        if (!ModelState.IsValid) return View(model);
        try { await accounts.IssueAsync(model.Id, model.Username, model.Password); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); return View(model); }
        TempData["Success"] = "Đã cấp tài khoản. Hãy bàn giao tên đăng nhập và mật khẩu vừa nhập cho sinh viên; sinh viên có thể đổi mật khẩu sau khi đăng nhập.";
        return RedirectToAction(nameof(IssueAccount), new { id = model.Id });
    }

    private async Task LoadClubs() => ViewBag.Clubs = new SelectList(await db.Clubs.OrderBy(x => x.ClubName).ToListAsync(), "Id", "ClubName");
    private async Task<bool> CanManage(int id) { if (User.IsInRole(AppRoles.Admin)) return true; var current = await scope.MemberAsync(User); return current != null && await db.MembershipApplications.AnyAsync(x => x.Id == id && x.ClubId == current.ClubId); }
}
