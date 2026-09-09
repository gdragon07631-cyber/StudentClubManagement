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

[Authorize]
public class MembersController(ApplicationDbContext db, IAccessScopeService scope) : Controller
{
    public async Task<IActionResult> Index(string? q, int? clubId, string? status, string sort = "name")
    {
        var current = await scope.MemberAsync(User); if (!User.IsInRole(AppRoles.Admin) && current == null) return Forbid(); var query = db.Members.Include(x => x.Club).AsQueryable();
        if (User.IsInRole(AppRoles.Member) && current != null) query = query.Where(x => x.Id == current.Id);
        else if (User.IsInRole(AppRoles.ClubLeader) && current != null) query = query.Where(x => x.ClubId == current.ClubId);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.FullName.Contains(q) || x.StudentCode.Contains(q));
        if (User.IsInRole(AppRoles.Admin) && clubId != null) query = query.Where(x => x.ClubId == clubId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.MemberStatus == status);
        query = sort switch { "studentCode" => query.OrderBy(x => x.StudentCode), "newest" => query.OrderByDescending(x => x.JoinedAt), _ => query.OrderBy(x => x.FullName) };
        ViewBag.Clubs = new SelectList(await db.Clubs.OrderBy(x => x.ClubName).ToListAsync(), "Id", "ClubName", clubId);
        ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(await query.ToListAsync());
    }
    public async Task<IActionResult> Details(int id) => await scope.CanAccessMemberAsync(User, id) ? View(await db.Members.Include(x => x.Club).Include(x => x.Skills).SingleAsync(x => x.Id == id)) : Forbid();
    [Authorize(Roles = AppRoles.Admin), HttpGet] public async Task<IActionResult> Create() { await LoadClubs(); return View(); }
    [Authorize(Roles = AppRoles.Admin), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member model)
    {
        ModelState.Remove(nameof(Member.Club)); ModelState.Remove(nameof(Member.Department));
        if (!await db.Clubs.AnyAsync(x => x.Id == model.ClubId)) ModelState.AddModelError(nameof(model.ClubId), "Vui lòng chọn câu lạc bộ hợp lệ.");
        if (model.UserId != null && await db.Members.AnyAsync(x => x.UserId == model.UserId)) ModelState.AddModelError(nameof(model.UserId), "Tài khoản này đã được liên kết với thành viên khác.");
        if (!ModelState.IsValid) { await LoadClubs(); return View(model); }
        model.DepartmentId = null; db.Members.Add(model);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { ModelState.AddModelError(nameof(model.StudentCode), "MSSV đã tồn tại trong câu lạc bộ."); await LoadClubs(); return View(model); }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await scope.CanAccessMemberAsync(User, id)) return Forbid(); var x = await db.Members.FindAsync(id); if (x == null) return NotFound(); await LoadClubs();
        return View(new MemberEditViewModel { Id = x.Id, UserId = x.UserId, FullName = x.FullName, Email = x.Email, Phone = x.Phone, ClassName = x.ClassName, Course = x.Course, Position = x.Position, ClubId = x.ClubId, MemberStatus = x.MemberStatus });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MemberEditViewModel model)
    {
        if (!await scope.CanAccessMemberAsync(User, model.Id)) return Forbid(); var x = await db.Members.FindAsync(model.Id); if (x == null) return NotFound();
        if (User.IsInRole(AppRoles.Admin) && !await db.Clubs.AnyAsync(c => c.Id == model.ClubId)) ModelState.AddModelError(nameof(model.ClubId), "Vui lòng chọn câu lạc bộ hợp lệ.");
        if (User.IsInRole(AppRoles.Admin) && model.UserId != null && await db.Members.AnyAsync(m => m.Id != model.Id && m.UserId == model.UserId)) ModelState.AddModelError(nameof(model.UserId), "Tài khoản này đã được liên kết với thành viên khác.");
        if (!ModelState.IsValid) { await LoadClubs(); return View(model); }
        x.FullName = model.FullName; x.Email = model.Email; x.Phone = model.Phone; x.ClassName = model.ClassName; x.Course = model.Course;
        if (!User.IsInRole(AppRoles.Member)) x.Position = model.Position;
        if (User.IsInRole(AppRoles.Admin)) { x.UserId = model.UserId; x.ClubId = model.ClubId; x.DepartmentId = null; x.MemberStatus = model.MemberStatus; }
        await db.SaveChangesAsync(); TempData["Success"] = "Đã cập nhật hồ sơ thành viên."; return RedirectToAction(nameof(Details), new { id = x.Id });
    }
    [Authorize(Roles = AppRoles.Admin), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status)
    {
        if (status is not (DomainStatus.Active or "INACTIVE")) return BadRequest(); var x = await db.Members.FindAsync(id); if (x == null) return NotFound();
        x.MemberStatus = status; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
        // TODO(BUSINESS_RULE_CONFIRMATION): dùng trạng thái thay vì xóa hồ sơ cho tới khi quy tắc rời CLB được chốt.
    }
    private async Task LoadClubs() { ViewBag.Clubs = new SelectList(await db.Clubs.OrderBy(x => x.ClubName).ToListAsync(), "Id", "ClubName"); ViewBag.Users = new SelectList(await db.Users.OrderBy(x => x.UserName).ToListAsync(), "Id", "UserName"); }
}
