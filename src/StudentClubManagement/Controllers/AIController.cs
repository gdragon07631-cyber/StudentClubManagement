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
public class AIController(IAiService ai, IAccessScopeService scope, UserManager<ApplicationUser> users, ApplicationDbContext db) : Controller
{
    [HttpGet] public IActionResult Chat() => View();
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Chat(string question) { if (string.IsNullOrWhiteSpace(question)) { ModelState.AddModelError("", "Vui lòng nhập câu hỏi."); return View(); } var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException(); return View(new AiResultViewModel("Chatbot", await ai.ChatAsync(u, question))); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Recommend(int taskId) { var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException(); var m = await scope.MemberAsync(User); if (User.IsInRole(AppRoles.ClubLeader) && (m == null || !await db.Tasks.AnyAsync(x => x.Id == taskId && ((x.Activity != null && x.Activity.ClubId == m.ClubId) || (x.Meeting != null && x.Meeting.ClubId == m.ClubId) || x.Assignments.Any(a => a.Member.ClubId == m.ClubId))))) return Forbid(); var result = await ai.RecommendAsync(u, taskId, User.IsInRole(AppRoles.ClubLeader) ? m!.ClubId : null); ViewBag.TaskId = taskId; return View("Recommendations", result); }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Participation(int memberId) { if (!await scope.CanAccessMemberAsync(User, memberId)) return Forbid(); var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException(); return View("Chat", new AiResultViewModel("Đánh giá mức tham gia", await ai.EvaluateParticipationAsync(u, memberId))); }
}
