using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class MeetingsController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status, string sort = "newest")
    {
        var cards = await api.GetAsync<List<MeetingCard>>(ApiClient.Url("api/meetings", ("q", q), ("status", status), ("sort", sort)));
        ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(cards);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Create() { await LoadClubs(); return View(); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Meeting model)
    {
        ModelState.Remove(nameof(Meeting.Club));
        if (ModelState.IsValid) { var r = await api.PostAsync("api/meetings", ToInput(model)); if (r.Success) return RedirectToAction(nameof(Index)); r.AddTo(ModelState); }
        await LoadClubs(); return View(model);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet]
    public async Task<IActionResult> Edit(int id) { var x = await api.GetAsync<Meeting>($"api/meetings/{id}"); await LoadClubs(); return View(x); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Meeting model)
    {
        ModelState.Remove(nameof(Meeting.Club)); if (id != model.Id) return BadRequest();
        if (ModelState.IsValid) { var r = await api.PutAsync($"api/meetings/{id}", ToInput(model)); if (r.Success) return RedirectToAction(nameof(Index)); r.AddTo(ModelState); }
        await LoadClubs(); return View(model);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(int id, int memberId, string status)
    {
        var r = await api.PostAsync($"api/meetings/{id}/attendance", new AttendanceInput { MemberId = memberId, Status = status ?? "" });
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
    private static MeetingInput ToInput(Meeting m) => new() { ClubId = m.ClubId, Title = m.Title, MeetingTime = m.MeetingTime, Location = m.Location, Agenda = m.Agenda, Minutes = m.Minutes, Decisions = m.Decisions, Status = m.Status };
    private async Task LoadClubs() => ViewBag.Clubs = new SelectList(await api.GetAsync<List<ScheduleClubOption>>("api/meetings/clubs"), "Id", "ClubName");
}
