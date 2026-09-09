using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Authorization;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class PointsController(ApplicationDbContext db, IAccessScopeService scope) : Controller
{
    public async Task<IActionResult> Index(string sort = "newest") { var m = await scope.MemberAsync(User); if (!User.IsInRole(AppRoles.Admin) && m == null) return Forbid(); var query = db.PointHistory.Include(x => x.Member).Include(x => x.PointRule).AsQueryable(); if (User.IsInRole(AppRoles.Member)) query = query.Where(x => x.MemberId == m!.Id); else if (User.IsInRole(AppRoles.ClubLeader)) query = query.Where(x => x.Member.ClubId == m!.ClubId); query = sort == "points" ? query.OrderByDescending(x => x.Points) : query.OrderByDescending(x => x.RecordedAt); ViewBag.Sort = sort; return View(await query.ToListAsync()); }
    [Authorize(Roles = AppRoles.Admin), HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Record(PointHistory model) { ModelState.Remove(nameof(PointHistory.Member)); ModelState.Remove(nameof(PointHistory.PointRule)); if (!ModelState.IsValid) { TempData["Error"] = "Dữ liệu điểm không hợp lệ."; return RedirectToAction(nameof(Index)); } db.PointHistory.Add(model); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}
