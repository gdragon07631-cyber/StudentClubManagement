using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/notifications"), Authorize]
public class NotificationsController(ApplicationDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index() { var id = (await users.GetUserAsync(User))!.Id; return Ok(await db.Notifications.Where(x => x.UserId == id).OrderByDescending(x => x.CreatedAt).ToListAsync()); }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> Read(int id) { var uid = (await users.GetUserAsync(User))!.Id; var n = await db.Notifications.SingleOrDefaultAsync(x => x.Id == id && x.UserId == uid); if (n == null) return NotFound(); n.IsRead = true; await db.SaveChangesAsync(); return NoContent(); }
}
