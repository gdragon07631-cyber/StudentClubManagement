using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/clubs"), Authorize(Roles = AppRoles.Admin)]
public class ClubsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
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
        return Ok(clubs);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var club = await db.Clubs.FindAsync(id);
        return club == null ? NotFound() : Ok(new ClubFormViewModel { Id = club.Id, ClubName = club.ClubName, Description = club.Description });
    }

    [HttpPost]
    public async Task<IActionResult> Create(ClubFormViewModel model)
    {
        if (await db.Clubs.AnyAsync(x => x.ClubName == model.ClubName))
            return BadRequest(new ApiError("Tên câu lạc bộ đã tồn tại."));

        db.Clubs.Add(new Club { ClubName = model.ClubName.Trim(), Description = model.Description?.Trim() });
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, ClubFormViewModel model)
    {
        if (id != model.Id) return BadRequest(new ApiError("Dữ liệu không hợp lệ."));
        if (await db.Clubs.AnyAsync(x => x.Id != id && x.ClubName == model.ClubName))
            return BadRequest(new ApiError("Tên câu lạc bộ đã tồn tại."));

        var club = await db.Clubs.FindAsync(id);
        if (club == null) return NotFound();
        club.ClubName = model.ClubName.Trim();
        club.Description = model.Description?.Trim();
        await db.SaveChangesAsync();
        return NoContent();
    }
}
