using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class ClubsController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index() => View(await api.GetAsync<List<ClubListItemViewModel>>("api/clubs"));

    [HttpGet]
    public IActionResult Create() => View(new ClubFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClubFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var r = await api.PostAsync("api/clubs", model);
        if (!r.Success) { r.AddTo(ModelState); return View(model); }
        TempData["Success"] = "Đã thêm câu lạc bộ mới.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id) => View(await api.GetAsync<ClubFormViewModel>($"api/clubs/{id}"));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClubFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var r = await api.PutAsync($"api/clubs/{id}", model);
        if (!r.Success) { r.AddTo(ModelState); return View(model); }
        TempData["Success"] = "Đã cập nhật câu lạc bộ.";
        return RedirectToAction(nameof(Index));
    }
}
