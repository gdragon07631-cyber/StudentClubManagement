using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class ActivitiesController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status, string sort = "newest")
    {
        var cards = await api.GetAsync<List<ActivityCard>>(ApiClient.Url("api/activities", ("q", q), ("status", status), ("sort", sort)));
        ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(cards);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Create() { await LoadClubs(); return View(); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Activity model)
    {
        ModelState.Remove(nameof(Activity.Club)); ValidateDates(model);
        if (ModelState.IsValid) { var r = await api.PostAsync("api/activities", ToInput(model)); if (r.Success) return RedirectToAction(nameof(Index)); r.AddTo(ModelState); }
        await LoadClubs(); return View(model);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet]
    public async Task<IActionResult> Edit(int id) { var x = await api.GetAsync<Activity>($"api/activities/{id}"); await LoadClubs(); return View(x); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Activity model)
    {
        ModelState.Remove(nameof(Activity.Club)); if (id != model.Id) return BadRequest(); ValidateDates(model);
        if (ModelState.IsValid) { var r = await api.PutAsync($"api/activities/{id}", ToInput(model)); if (r.Success) return RedirectToAction(nameof(Index)); r.AddTo(ModelState); }
        await LoadClubs(); return View(model);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelActivity(int id) { await api.PostAsync($"api/activities/{id}/cancel-activity"); return RedirectToAction(nameof(Index)); }
    [Authorize(Roles = AppRoles.Member), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id)
    {
        var r = await api.PostAsync($"api/activities/{id}/register");
        if (r.Success) TempData["Success"] = "Đăng ký hoạt động thành công."; else TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = AppRoles.Member), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var r = await api.DeleteAsync($"api/activities/{id}/register");
        if (!r.Success) TempData["Error"] = "Không tìm thấy đăng ký hoạt động.";
        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(int id, int memberId, string status)
    {
        var r = await api.PostAsync($"api/activities/{id}/attendance", new AttendanceInput { MemberId = memberId, Status = status ?? "" });
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
    private static ActivityInput ToInput(Activity m) => new() { ClubId = m.ClubId, ActivityName = m.ActivityName, Description = m.Description, StartTime = m.StartTime, EndTime = m.EndTime, Location = m.Location, Status = m.Status, Result = m.Result };
    private void ValidateDates(Activity model) { if (model.EndTime < model.StartTime) ModelState.AddModelError(nameof(model.EndTime), "Thời gian kết thúc phải sau thời gian bắt đầu."); }
    private async Task LoadClubs() => ViewBag.Clubs = new SelectList(await api.GetAsync<List<ScheduleClubOption>>("api/activities/clubs"), "Id", "ClubName");
}
