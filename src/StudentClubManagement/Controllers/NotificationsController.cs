using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class NotificationsController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index() { var id = (await users.GetUserAsync(User))!.Id; return View(await db.Notifications.Where(x => x.UserId == id).OrderByDescending(x => x.CreatedAt).ToListAsync()); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Read(int id) { var uid = (await users.GetUserAsync(User))!.Id; var n = await db.Notifications.SingleOrDefaultAsync(x => x.Id == id && x.UserId == uid); if (n == null) return NotFound(); n.IsRead = true; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}

