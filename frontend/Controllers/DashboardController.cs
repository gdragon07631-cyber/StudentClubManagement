using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class DashboardController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = await api.GetAsync<DashboardViewModel>("api/dashboard");
        if (User.IsInRole(AppRoles.Admin)) return View("Admin", vm);
        if (User.IsInRole(AppRoles.ClubLeader)) return View("DepartmentLeader", vm);
        return View("Member", vm);
    }
}
