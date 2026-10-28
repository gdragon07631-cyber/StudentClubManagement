using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class UsersController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        var data = await api.GetAsync<UserListResponse>("api/users");
        ViewBag.Members = data.Members;
        ViewBag.CurrentRoles = data.CurrentRoles;
        return View(data.Users);
    }
    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (model.Role is not (AppRoles.Admin or AppRoles.ClubLeader or AppRoles.Member)) ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ.");
        if (!ModelState.IsValid) return View(model);
        var r = await api.PostAsync("api/users", model);
        if (!r.Success) { r.AddTo(ModelState); return View(model); }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(int id, string role, int? memberId)
    {
        var r = await api.PostAsync<SetRoleResponse>($"api/users/{id}/role", new SetRoleRequest(role, memberId));
        if (!r.Success) { TempData["Error"] = r.Error; return RedirectToAction(nameof(Index)); }
        if (r.Data?.IsCurrentUser == true)
        {
            // JWT hiện tại đã cũ so với vai trò mới nên buộc đăng nhập lại.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Đã lưu vai trò. Vui lòng đăng nhập lại để sử dụng quyền mới.";
            return Redirect("/Account/Login");
        }
        TempData["Success"] = "Đã lưu vai trò. Người dùng cần đăng nhập lại để sử dụng quyền mới.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await api.PostAsync($"api/users/{id}/toggle-status");
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
}
