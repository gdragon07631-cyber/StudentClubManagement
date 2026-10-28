using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Authorization;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/ai"), Authorize]
public class AIController(IAiService ai, IAccessScopeService scope, UserManager<ApplicationUser> users, ApplicationDbContext db) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(AiChatRequest request)
    {
        var question = request.Question;
        if (string.IsNullOrWhiteSpace(question)) return BadRequest(new ApiError("Vui lòng nhập câu hỏi."));
        var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException();
        return Ok(new AiResultViewModel("Chatbot", await ai.ChatAsync(u, question)));
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost("recommend/{taskId:int}")]
    public async Task<IActionResult> Recommend(int taskId)
    {
        var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException(); var m = await scope.MemberAsync(User);
        if (User.IsInRole(AppRoles.ClubLeader) && (m == null || !await db.Tasks.AnyAsync(x => x.Id == taskId && ((x.Activity != null && x.Activity.ClubId == m.ClubId) || (x.Meeting != null && x.Meeting.ClubId == m.ClubId) || x.Assignments.Any(a => a.Member.ClubId == m.ClubId))))) return Forbid();
        var result = await ai.RecommendAsync(u, taskId, User.IsInRole(AppRoles.ClubLeader) ? m!.ClubId : null);
        return Ok(result);
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost("participation/{memberId:int}")]
    public async Task<IActionResult> Participation(int memberId)
    {
        if (!await scope.CanAccessMemberAsync(User, memberId)) return Forbid();
        var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException();
        return Ok(new AiResultViewModel("Đánh giá mức tham gia", await ai.EvaluateParticipationAsync(u, memberId)));
    }
}
