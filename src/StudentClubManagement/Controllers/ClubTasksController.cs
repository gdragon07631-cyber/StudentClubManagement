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
public class ClubTasksController(ApplicationDbContext db, IAccessScopeService scope, IClubService service) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status, string sort = "due")
    {
        var m = await scope.MemberAsync(User); if (!User.IsInRole(AppRoles.Admin) && m == null) return Forbid(); var query = db.Tasks.Include(x => x.Activity).Include(x => x.Meeting).Include(x => x.Assignments).ThenInclude(x => x.Member).AsQueryable();
        if (User.IsInRole(AppRoles.Member) && m != null) query = query.Where(x => x.Assignments.Any(a => a.MemberId == m.Id));
        else if (User.IsInRole(AppRoles.ClubLeader) && m != null) query = query.Where(x => (x.Activity != null && x.Activity.ClubId == m.ClubId) || (x.Meeting != null && x.Meeting.ClubId == m.ClubId) || x.Assignments.Any(a => a.Member.ClubId == m.ClubId));
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Title.Contains(q)); if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = sort switch { "title" => query.OrderBy(x => x.Title), "progress" => query.OrderByDescending(x => x.Progress), _ => query.OrderBy(x => x.DueDate) };
        ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort;
        var candidates = db.Members.Where(x => x.MemberStatus == DomainStatus.Active); if (User.IsInRole(AppRoles.ClubLeader) && m != null) candidates = candidates.Where(x => x.ClubId == m.ClubId);
        ViewBag.Members = new SelectList(await candidates.OrderBy(x => x.FullName).ToListAsync(), "Id", "FullName"); return View(await query.ToListAsync());
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Create() { await LoadSources(); return View(); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClubTask model) { ModelState.Remove(nameof(ClubTask.Activity)); ModelState.Remove(nameof(ClubTask.Meeting)); await ValidateSourceScope(model); if (!ModelState.IsValid) { await LoadSources(); return View(model); } db.Tasks.Add(model); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Edit(int id) { if (!await CanManage(id)) return Forbid(); var x = await db.Tasks.FindAsync(id); if (x == null) return NotFound(); await LoadSources(); return View(x); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClubTask model) { if (id != model.Id) return BadRequest(); if (!await CanManage(id)) return Forbid(); ModelState.Remove(nameof(ClubTask.Activity)); ModelState.Remove(nameof(ClubTask.Meeting)); await ValidateSourceScope(model); if (!ModelState.IsValid) { await LoadSources(); return View(model); } var x = await db.Tasks.FindAsync(id); if (x == null) return NotFound(); x.Title = model.Title; x.Description = model.Description; x.Priority = model.Priority; x.StartDate = model.StartDate; x.DueDate = model.DueDate; x.Status = model.Status; x.ActivityId = model.ActivityId; x.MeetingId = model.MeetingId; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, int memberId)
    {
        if (!await CanManage(id) || !await scope.CanAccessMemberAsync(User, memberId)) return Forbid();
        if (await db.TaskAssignments.AnyAsync(x => x.TaskId == id && x.MemberId == memberId)) { TempData["Error"] = "Thành viên đã được phân công nhiệm vụ này."; return RedirectToAction(nameof(Index)); }
        db.TaskAssignments.Add(new() { TaskId = id, MemberId = memberId }); await db.SaveChangesAsync(); TempData["Success"] = "Đã xác nhận phân công."; return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = AppRoles.Member), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Progress(int id, int progress) { var m = await scope.MemberAsync(User); if (m == null) return Forbid(); try { await service.UpdateTaskProgressAsync(id, m.Id, progress); } catch (ArgumentOutOfRangeException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Index)); }
    private async Task LoadSources()
    {
        var activities = db.Activities.AsQueryable(); var meetings = db.Meetings.AsQueryable();
        if (User.IsInRole(AppRoles.ClubLeader)) { var m = await scope.MemberAsync(User); if (m == null) { ViewBag.Activities = new SelectList(Array.Empty<Activity>()); ViewBag.Meetings = new SelectList(Array.Empty<Meeting>()); return; } activities = activities.Where(x => x.ClubId == m.ClubId); meetings = meetings.Where(x => x.ClubId == m.ClubId); }
        ViewBag.Activities = new SelectList(await activities.OrderBy(x => x.ActivityName).ToListAsync(), "Id", "ActivityName"); ViewBag.Meetings = new SelectList(await meetings.OrderBy(x => x.Title).ToListAsync(), "Id", "Title");
    }
    private async Task ValidateSourceScope(ClubTask model)
    {
        if (!User.IsInRole(AppRoles.ClubLeader)) return;
        var m = await scope.MemberAsync(User);
        var activityAllowed = model.ActivityId != null && m != null && await db.Activities.AnyAsync(x => x.Id == model.ActivityId && x.ClubId == m.ClubId);
        var meetingAllowed = model.MeetingId != null && m != null && await db.Meetings.AnyAsync(x => x.Id == model.MeetingId && x.ClubId == m.ClubId);
        if (!activityAllowed && !meetingAllowed) ModelState.AddModelError("", "Trưởng câu lạc bộ phải chọn hoạt động hoặc cuộc họp thuộc câu lạc bộ của mình.");
    }
    private async Task<bool> CanManage(int taskId) { if (User.IsInRole(AppRoles.Admin)) return true; var m = await scope.MemberAsync(User); return m != null && await db.Tasks.AnyAsync(x => x.Id == taskId && ((x.Activity != null && x.Activity.ClubId == m.ClubId) || (x.Meeting != null && x.Meeting.ClubId == m.ClubId) || x.Assignments.Any(a => a.Member.ClubId == m.ClubId))); }
}
