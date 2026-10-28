using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Authorization;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/activities"), Authorize]
public class ActivitiesController(ApplicationDbContext db, IClubService service, IAccessScopeService scope) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? status, string sort = "newest")
    {
        var current = await scope.MemberAsync(User); var query = db.Activities.AsNoTracking().AsQueryable();
        if (!User.IsInRole(AppRoles.Admin)) { if (current == null) return Forbid(); query = query.Where(x => x.ClubId == current.ClubId); }
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.ActivityName.Contains(q) || (x.Location != null && x.Location.Contains(q)));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = sort == "name" ? query.OrderBy(x => x.ActivityName) : query.OrderByDescending(x => x.StartTime);
        var activities = await query.ToListAsync();
        var cards = activities.Select(a => new ActivityCard { Activity = a }).ToList();
        if (!User.IsInRole(AppRoles.Member) && activities.Count > 0)
        {
            var ids = activities.Select(a => a.Id).ToList();
            var clubIds = activities.Select(a => a.ClubId).Distinct().ToList();
            var attendances = await db.ActivityAttendance.AsNoTracking().Include(a => a.Member).Where(a => ids.Contains(a.ActivityId)).ToListAsync();
            var members = await db.Members.AsNoTracking().Where(m => clubIds.Contains(m.ClubId) && m.MemberStatus == DomainStatus.Active).OrderBy(m => m.FullName).ToListAsync();
            foreach (var card in cards)
            {
                card.Attendances = attendances.Where(a => a.ActivityId == card.Activity.Id).ToList();
                var attendedIds = card.Attendances.Select(a => a.MemberId).ToHashSet();
                card.Unrecorded = members.Where(m => m.ClubId == card.Activity.ClubId && !attendedIds.Contains(m.Id)).ToList();
            }
        }
        return Ok(cards);
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet("clubs")]
    public async Task<IActionResult> Clubs() => Ok(await db.Clubs.AsNoTracking().OrderBy(x => x.ClubName).Select(x => new ScheduleClubOption { Id = x.Id, ClubName = x.ClubName }).ToListAsync());

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var x = await db.Activities.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); return Ok(x);
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost]
    public async Task<IActionResult> Create(ActivityInput input)
    {
        var model = ToEntity(input);
        var error = await SetAndValidateClub(model, input) ?? ValidateDates(model); if (error != null) return BadRequest(new ApiError(error));
        db.Activities.Add(model); await db.SaveChangesAsync(); return Ok(new { model.Id });
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, ActivityInput input)
    {
        var x = await db.Activities.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid();
        var model = ToEntity(input);
        var error = await SetAndValidateClub(model, input) ?? ValidateDates(model); if (error != null) return BadRequest(new ApiError(error));
        x.ClubId = model.ClubId; x.ActivityName = model.ActivityName; x.Description = model.Description; x.StartTime = model.StartTime; x.EndTime = model.EndTime; x.Location = model.Location; x.Status = model.Status; x.Result = model.Result;
        await db.SaveChangesAsync(); return NoContent();
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost("{id:int}/cancel-activity")]
    public async Task<IActionResult> CancelActivity(int id)
    {
        var x = await db.Activities.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); x.Status = "CANCELLED"; await db.SaveChangesAsync(); return NoContent();
    }

    [Authorize(Roles = AppRoles.Member), HttpPost("{id:int}/register")]
    public async Task<IActionResult> Register(int id)
    {
        var m = await scope.MemberAsync(User); if (m == null) return Forbid();
        try { await service.RegisterActivityAsync(id, m.Id); return NoContent(); } catch (InvalidOperationException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [Authorize(Roles = AppRoles.Member), HttpDelete("{id:int}/register")]
    public async Task<IActionResult> Cancel(int id)
    {
        var m = await scope.MemberAsync(User); if (m == null) return Forbid();
        try { await service.CancelActivityRegistrationAsync(id, m.Id); return NoContent(); } catch (InvalidOperationException) { return BadRequest(new ApiError("Không tìm thấy đăng ký hoạt động.")); }
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost("{id:int}/attendance")]
    public async Task<IActionResult> Attendance(int id, AttendanceInput input)
    {
        var activity = await db.Activities.FindAsync(id); if (activity == null) return NotFound();
        if (!await CanManage(activity.ClubId) || !await scope.CanAccessMemberAsync(User, input.MemberId)) return Forbid();
        try { await service.RecordActivityAttendanceAsync(id, input.MemberId, input.Status); return NoContent(); } catch (InvalidOperationException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    static Activity ToEntity(ActivityInput i) => new() { ClubId = i.ClubId, ActivityName = i.ActivityName, Description = i.Description, StartTime = i.StartTime, EndTime = i.EndTime, Location = i.Location, Status = i.Status ?? "PLANNED", Result = i.Result };
    static string? ValidateDates(Activity model) => model.EndTime < model.StartTime ? "Thời gian kết thúc phải sau thời gian bắt đầu." : null;
    private async Task<string?> SetAndValidateClub(Activity model, ActivityInput input)
    {
        if (User.IsInRole(AppRoles.ClubLeader)) { var current = await scope.MemberAsync(User); if (current == null) return "Tài khoản chưa có hồ sơ sinh viên."; model.ClubId = current.ClubId; }
        if (!await db.Clubs.AnyAsync(x => x.Id == model.ClubId)) return "Vui lòng chọn câu lạc bộ hợp lệ.";
        return null;
    }
    private async Task<bool> CanManage(int clubId) { if (User.IsInRole(AppRoles.Admin)) return true; var current = await scope.MemberAsync(User); return current != null && current.ClubId == clubId; }
}
