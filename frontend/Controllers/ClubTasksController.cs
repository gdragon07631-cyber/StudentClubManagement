using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class ClubTasksController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status, string sort = "due")
    {
        var data = await api.GetAsync<TaskListResponse>(ApiClient.Url("api/club-tasks", ("q", q), ("status", status), ("sort", sort)));
        ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort;
        ViewBag.Members = new SelectList(data.Members, "Id", "Name");
        return View(data.Tasks);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet] public async Task<IActionResult> Create() { await LoadSources(); return View(); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClubTask model)
    {
        ModelState.Remove(nameof(ClubTask.Activity)); ModelState.Remove(nameof(ClubTask.Meeting));
        if (ModelState.IsValid)
        {
            var r = await api.PostAsync("api/club-tasks", model);
            if (r.Success) return RedirectToAction(nameof(Index));
            r.AddTo(ModelState);
        }
        await LoadSources(); return View(model);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var data = await api.GetAsync<TaskEditResponse>($"api/club-tasks/{id}");
        SetSources(data.Sources); return View(data.Task);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClubTask model)
    {
        if (id != model.Id) return BadRequest();
        ModelState.Remove(nameof(ClubTask.Activity)); ModelState.Remove(nameof(ClubTask.Meeting));
        if (ModelState.IsValid)
        {
            var r = await api.PutAsync($"api/club-tasks/{id}", model);
            if (r.Success) return RedirectToAction(nameof(Index));
            r.AddTo(ModelState);
        }
        await LoadSources(); return View(model);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, int memberId)
    {
        var r = await api.PostAsync($"api/club-tasks/{id}/assign", new AssignTaskRequest(memberId));
        if (r.Success) TempData["Success"] = "Đã xác nhận phân công."; else TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = AppRoles.Member), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Progress(int id, int progress)
    {
        var r = await api.PostAsync($"api/club-tasks/{id}/progress", new UpdateTaskProgressRequest(progress));
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
    private async Task LoadSources() => SetSources(await api.GetAsync<TaskSourcesResponse>("api/club-tasks/sources"));
    private void SetSources(TaskSourcesResponse s) { ViewBag.Activities = new SelectList(s.Activities, "Id", "Name"); ViewBag.Meetings = new SelectList(s.Meetings, "Id", "Name"); }
}
