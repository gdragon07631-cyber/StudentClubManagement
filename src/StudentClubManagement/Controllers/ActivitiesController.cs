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
public class ActivitiesController(ApplicationDbContext db, IClubService service, IAccessScopeService scope) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status, string sort = "newest")
    {
        var current = await scope.MemberAsync(User); var query = db.Activities.AsQueryable(); if (!User.IsInRole(AppRoles.Admin)) { if (current == null) return Forbid(); query = query.Where(x => x.ClubId == current.ClubId); } if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.ActivityName.Contains(q) || (x.Location != null && x.Location.Contains(q)));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status); query = sort == "name" ? query.OrderBy(x => x.ActivityName) : query.OrderByDescending(x => x.StartTime);
        ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(await query.ToListAsync());
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Create() { await LoadClubs(); return View(); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Activity model) { ModelState.Remove(nameof(Activity.Club)); await SetAndValidateClub(model); ValidateDates(model); if (!ModelState.IsValid) { await LoadClubs(); return View(model); } db.Activities.Add(model); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Edit(int id) { var x = await db.Activities.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); await LoadClubs(); return View(x); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Activity model) { ModelState.Remove(nameof(Activity.Club)); if (id != model.Id) return BadRequest(); var x = await db.Activities.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); await SetAndValidateClub(model); ValidateDates(model); if (!ModelState.IsValid) { await LoadClubs(); return View(model); } x.ClubId = model.ClubId; x.ActivityName = model.ActivityName; x.Description = model.Description; x.StartTime = model.StartTime; x.EndTime = model.EndTime; x.Location = model.Location; x.Status = model.Status; x.Result = model.Result; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> CancelActivity(int id) { var x = await db.Activities.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); x.Status = "CANCELLED"; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Member), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id) { var m = await scope.MemberAsync(User); if (m == null) return Forbid(); try { await service.RegisterActivityAsync(id, m.Id); TempData["Success"] = "Đăng ký hoạt động thành công."; } catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Member), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id) { var m = await scope.MemberAsync(User); if (m == null) return Forbid(); try { await service.CancelActivityRegistrationAsync(id, m.Id); } catch (InvalidOperationException) { TempData["Error"] = "Không tìm thấy đăng ký hoạt động."; } return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(int id, int memberId, string status) { var activity = await db.Activities.FindAsync(id); if (activity == null) return NotFound(); if (!await CanManage(activity.ClubId) || !await scope.CanAccessMemberAsync(User, memberId)) return Forbid(); try { await service.RecordActivityAttendanceAsync(id, memberId, status); } catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Index)); }
    private void ValidateDates(Activity model) { if (model.EndTime < model.StartTime) ModelState.AddModelError(nameof(model.EndTime), "Thời gian kết thúc phải sau thời gian bắt đầu."); }
    private async Task LoadClubs() => ViewBag.Clubs = new SelectList(await db.Clubs.OrderBy(x => x.ClubName).ToListAsync(), "Id", "ClubName");
    private async Task SetAndValidateClub(Activity model) { if (User.IsInRole(AppRoles.ClubLeader)) { var current = await scope.MemberAsync(User); if (current == null) { ModelState.AddModelError("", "Tài khoản chưa có hồ sơ sinh viên."); return; } model.ClubId = current.ClubId; } if (!await db.Clubs.AnyAsync(x => x.Id == model.ClubId)) ModelState.AddModelError(nameof(model.ClubId), "Vui lòng chọn câu lạc bộ hợp lệ."); }
    private async Task<bool> CanManage(int clubId) { if (User.IsInRole(AppRoles.Admin)) return true; var current = await scope.MemberAsync(User); return current != null && current.ClubId == clubId; }
}
