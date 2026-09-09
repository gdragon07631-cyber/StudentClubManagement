using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class ClubsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var clubs = await db.Clubs
            .OrderBy(x => x.ClubName)
            .Select(x => new ClubListItemViewModel(
                x.Id,
                x.ClubName,
                x.Description,
                db.Members.Count(m => m.ClubId == x.Id),
                db.Activities.Count(a => a.ClubId == x.Id),
                db.Meetings.Count(m => m.ClubId == x.Id)))
            .ToListAsync();
        return View(clubs);
    }

    [HttpGet]
    public IActionResult Create() => View(new ClubFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClubFormViewModel model)
    {
        if (await db.Clubs.AnyAsync(x => x.ClubName == model.ClubName))
            ModelState.AddModelError(nameof(model.ClubName), "Tên câu lạc bộ đã tồn tại.");
        if (!ModelState.IsValid) return View(model);

        db.Clubs.Add(new Club { ClubName = model.ClubName.Trim(), Description = model.Description?.Trim() });
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm câu lạc bộ mới.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var club = await db.Clubs.FindAsync(id);
        return club == null ? NotFound() : View(new ClubFormViewModel { Id = club.Id, ClubName = club.ClubName, Description = club.Description });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClubFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (await db.Clubs.AnyAsync(x => x.Id != id && x.ClubName == model.ClubName))
            ModelState.AddModelError(nameof(model.ClubName), "Tên câu lạc bộ đã tồn tại.");
        if (!ModelState.IsValid) return View(model);

        var club = await db.Clubs.FindAsync(id);
        if (club == null) return NotFound();
        club.ClubName = model.ClubName.Trim();
        club.Description = model.Description?.Trim();
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật câu lạc bộ.";
        return RedirectToAction(nameof(Index));
    }
}
