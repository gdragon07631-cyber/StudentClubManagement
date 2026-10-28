using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class MembersController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? q, int? clubId, string? status, string sort = "name")
    {
        var data = await api.GetAsync<MemberListResponse>(ApiClient.Url("api/members", ("q", q), ("clubId", clubId), ("status", status), ("sort", sort)));
        ViewBag.Clubs = new SelectList(data.Clubs, "Id", "ClubName", clubId);
        ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(data.Items);
    }
    public async Task<IActionResult> Details(int id) => View(await api.GetAsync<Member>($"api/members/{id}"));
    [Authorize(Roles = AppRoles.Admin), HttpGet] public async Task<IActionResult> Create() { await LoadLookups(); return View(); }
    [Authorize(Roles = AppRoles.Admin), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member model)
    {
        ModelState.Remove(nameof(Member.Club)); ModelState.Remove(nameof(Member.Department));
        if (!ModelState.IsValid) { await LoadLookups(); return View(model); }
        var body = new MemberCreateRequest
        {
            UserId = model.UserId, ClubId = model.ClubId, StudentCode = model.StudentCode, FullName = model.FullName, ClassName = model.ClassName,
            Course = model.Course, Position = model.Position, Phone = model.Phone, Email = model.Email, MemberStatus = model.MemberStatus
        };
        var r = await api.PostAsync("api/members", body);
        if (!r.Success) { r.AddTo(ModelState); await LoadLookups(); return View(model); }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var data = await api.GetAsync<MemberEditResponse>($"api/members/{id}/edit");
        SetLookups(data.Clubs, data.Users);
        return View(data.Member);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MemberEditViewModel model)
    {
        if (!ModelState.IsValid) { await LoadLookups(); return View(model); }
        var r = await api.PutAsync($"api/members/{model.Id}", model);
        if (!r.Success) { r.AddTo(ModelState); await LoadLookups(); return View(model); }
        TempData["Success"] = "Đã cập nhật hồ sơ thành viên."; return RedirectToAction(nameof(Details), new { id = model.Id });
    }
    [Authorize(Roles = AppRoles.Admin), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status)
    {
        var r = await api.PutAsync(ApiClient.Url($"api/members/{id}/status", ("status", status)));
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
    private async Task LoadLookups()
    {
        var data = await api.GetAsync<MemberFormLookups>("api/members/lookups");
        SetLookups(data.Clubs, data.Users);
    }
    private void SetLookups(List<ClubOption> clubs, List<UserOption> users)
    {
        ViewBag.Clubs = new SelectList(clubs, "Id", "ClubName"); ViewBag.Users = new SelectList(users, "Id", "UserName");
    }
}
