using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

public class MembershipApplicationsController(ApiClient api) : Controller
{
    [AllowAnonymous, HttpGet] public async Task<IActionResult> Apply() { await LoadClubs(); return View(); }
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(MembershipApplicationViewModel vm)
    {
        if (!ModelState.IsValid) { await LoadClubs(); return View(vm); }
        var r = await api.PostAsync("api/membership-applications", vm);
        if (!r.Success) { r.AddTo(ModelState); await LoadClubs(); return View(vm); }
        return RedirectToAction(nameof(Status), new { code = vm.StudentCode });
    }
    [AllowAnonymous] public async Task<IActionResult> Status(string code) => View((await api.GetAsync<ApplicationStatusResponse>(ApiClient.Url("api/membership-applications/status", ("code", code)))).Application);
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader)]
    public async Task<IActionResult> Index(string? q, int? clubId, string? status, string sort = "newest")
    {
        var data = await api.GetAsync<ApplicationListResponse>(ApiClient.Url("api/membership-applications", ("q", q), ("clubId", clubId), ("status", status), ("sort", sort)));
        ViewBag.Clubs = new SelectList(data.Clubs, "Id", "ClubName", clubId); ViewBag.Query = q; ViewBag.Status = status; ViewBag.Sort = sort; return View(data.Items);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var r = await api.PostAsync($"api/membership-applications/{id}/approve");
        if (!r.Success) { TempData["Error"] = r.Error; return RedirectToAction(nameof(Index)); }
        TempData["Success"] = "Đã duyệt đơn đăng ký. Bạn có thể cấp tài khoản cho sinh viên."; return RedirectToAction(nameof(IssueAccount), new { id });
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var r = await api.PostAsync($"api/membership-applications/{id}/reject", new RejectApplicationRequest(reason));
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet]
    public async Task<IActionResult> IssueAccount(int id) => View(await api.GetAsync<IssueAccountViewModel>($"api/membership-applications/{id}/issue-account"));

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> IssueAccount(IssueAccountViewModel model)
    {
        var info = await api.GetAsync<IssueAccountViewModel>($"api/membership-applications/{model.Id}/issue-account");
        model.FullName = info.FullName;
        model.ExistingUsername = null;
        if (info.ExistingUsername != null) return RedirectToAction(nameof(IssueAccount), new { id = model.Id });
        if (!ModelState.IsValid) return View(model);
        var r = await api.PostAsync($"api/membership-applications/{model.Id}/issue-account", model);
        if (!r.Success) { r.AddTo(ModelState); return View(model); }
        TempData["Success"] = "Đã cấp tài khoản. Hãy bàn giao tên đăng nhập và mật khẩu vừa nhập cho sinh viên; sinh viên có thể đổi mật khẩu sau khi đăng nhập.";
        return RedirectToAction(nameof(IssueAccount), new { id = model.Id });
    }

    private async Task LoadClubs()
    {
        var clubs = await api.GetAsync<List<ClubOption>>("api/membership-applications/clubs");
        ViewBag.Clubs = new SelectList(clubs, "Id", "ClubName");
    }
}
