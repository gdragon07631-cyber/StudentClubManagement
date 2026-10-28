using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;
using StudentClubManagement.Services;

namespace StudentClubManagement.Controllers;

[Authorize]
public class NotificationsController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index() => View(await api.GetAsync<List<Notification>>("api/notifications"));
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Read(int id) { await api.PostAsync($"api/notifications/{id}/read"); return RedirectToAction(nameof(Index)); }
}
