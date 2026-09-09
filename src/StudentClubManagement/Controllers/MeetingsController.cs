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
public class MeetingsController(ApplicationDbContext db, IClubService service, IAccessScopeService scope) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status, string sort = "newest") { var current = await scope.MemberAsync(User); var query = db.Meetings.AsQueryable(); if (!User.IsInRole(AppRoles.Admin)) { if (current == null) return Forbid(); query = query.Where(x => x.ClubId == current.ClubId); } if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Title.Contains(q) || (x.Location != null && x.Location.Contains(q))); if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status); query = sort == "title" ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.MeetingTime); ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(await query.ToListAsync()); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Create() { await LoadClubs(); return View(); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Meeting model) { ModelState.Remove(nameof(Meeting.Club)); await SetAndValidateClub(model); if (!ModelState.IsValid) { await LoadClubs(); return View(model); } db.Meetings.Add(model); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Edit(int id) { var x = await db.Meetings.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); await LoadClubs(); return View(x); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Meeting model) { ModelState.Remove(nameof(Meeting.Club)); if (id != model.Id) return BadRequest(); var x = await db.Meetings.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); await SetAndValidateClub(model); if (!ModelState.IsValid) { await LoadClubs(); return View(model); } x.ClubId = model.ClubId; x.Title = model.Title; x.MeetingTime = model.MeetingTime; x.Location = model.Location; x.Agenda = model.Agenda; x.Minutes = model.Minutes; x.Decisions = model.Decisions; x.Status = model.Status; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(int id, int memberId, string status) { var meeting = await db.Meetings.FindAsync(id); if (meeting == null) return NotFound(); if (!await CanManage(meeting.ClubId) || !await scope.CanAccessMemberAsync(User, memberId)) return Forbid(); try { await service.RecordMeetingAttendanceAsync(id, memberId, status); } catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Index)); }
    private async Task LoadClubs() => ViewBag.Clubs = new SelectList(await db.Clubs.OrderBy(x => x.ClubName).ToListAsync(), "Id", "ClubName");
    private async Task SetAndValidateClub(Meeting model) { if (User.IsInRole(AppRoles.ClubLeader)) { var current = await scope.MemberAsync(User); if (current == null) { ModelState.AddModelError("", "Tài khoản chưa có hồ sơ sinh viên."); return; } model.ClubId = current.ClubId; } if (!await db.Clubs.AnyAsync(x => x.Id == model.ClubId)) ModelState.AddModelError(nameof(model.ClubId), "Vui lòng chọn câu lạc bộ hợp lệ."); }
    private async Task<bool> CanManage(int clubId) { if (User.IsInRole(AppRoles.Admin)) return true; var current = await scope.MemberAsync(User); return current != null && current.ClubId == clubId; }
}
