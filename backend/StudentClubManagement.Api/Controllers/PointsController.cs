using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Authorization;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/points"), Authorize]
public class PointsController(ApplicationDbContext db, IAccessScopeService scope) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string sort = "newest")
    {
        var m = await scope.MemberAsync(User); if (!User.IsInRole(AppRoles.Admin) && m == null) return Forbid();
        var query = db.PointHistory.AsNoTracking().Include(x => x.Member).Include(x => x.PointRule).AsQueryable();
        if (User.IsInRole(AppRoles.Member)) query = query.Where(x => x.MemberId == m!.Id);
        else if (User.IsInRole(AppRoles.ClubLeader)) query = query.Where(x => x.Member.ClubId == m!.ClubId);
        query = sort == "points" ? query.OrderByDescending(x => x.Points) : query.OrderByDescending(x => x.RecordedAt);
        var response = new PointIndexResponse { Items = await query.ToListAsync() };
        if (User.IsInRole(AppRoles.Admin))
        {
            response.ActiveMembers = await db.Members.AsNoTracking().Where(x => x.MemberStatus == DomainStatus.Active).OrderBy(x => x.FullName)
                .Select(x => new PointMemberOption { Id = x.Id, FullName = x.FullName, StudentCode = x.StudentCode, ClubName = x.Club.ClubName }).ToListAsync();
            response.Rules = await db.PointRules.AsNoTracking().Where(r => r.Status == DomainStatus.Active).OrderBy(r => r.RuleName).ToListAsync();
        }
        return Ok(response);
    }

    [Authorize(Roles = AppRoles.Admin), HttpPost]
    public async Task<IActionResult> Record(PointRecordInput input)
    {
        if (!await db.Members.AnyAsync(x => x.Id == input.MemberId) || !await db.PointRules.AnyAsync(x => x.Id == input.PointRuleId)) return BadRequest(new ApiError("Dữ liệu điểm không hợp lệ."));
        db.PointHistory.Add(new PointHistory { MemberId = input.MemberId, PointRuleId = input.PointRuleId, SourceType = input.SourceType, SourceId = input.SourceId, Points = input.Points, Reason = input.Reason });
        await db.SaveChangesAsync(); return NoContent();
    }
}
