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

[ApiController, Route("api/club-tasks"), Authorize]
public class ClubTasksController(ApplicationDbContext db, IAccessScopeService scope, IClubService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? status, string sort = "due")
    {
        var m = await scope.MemberAsync(User); if (!User.IsInRole(AppRoles.Admin) && m == null) return Forbid(); var query = db.Tasks.Include(x => x.Activity).Include(x => x.Meeting).Include(x => x.Assignments).ThenInclude(x => x.Member).AsQueryable();
        if (User.IsInRole(AppRoles.Member) && m != null) query = query.Where(x => x.Assignments.Any(a => a.MemberId == m.Id));
        else if (User.IsInRole(AppRoles.ClubLeader) && m != null) query = query.Where(x => (x.Activity != null && x.Activity.ClubId == m.ClubId) || (x.Meeting != null && x.Meeting.ClubId == m.ClubId) || x.Assignments.Any(a => a.Member.ClubId == m.ClubId));
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Title.Contains(q)); if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = sort switch { "title" => query.OrderBy(x => x.Title), "progress" => query.OrderByDescending(x => x.Progress), _ => query.OrderBy(x => x.DueDate) };
        var candidates = db.Members.Where(x => x.MemberStatus == DomainStatus.Active); if (User.IsInRole(AppRoles.ClubLeader) && m != null) candidates = candidates.Where(x => x.ClubId == m.ClubId);
        var members = (await candidates.OrderBy(x => x.FullName).ToListAsync()).Select(x => new TaskLookupItem(x.Id, x.FullName)).ToList();
        return Ok(new TaskListResponse(await query.ToListAsync(), members));
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet("sources")]
    public async Task<IActionResult> Sources() => Ok(await LoadSources());

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost]
    public async Task<IActionResult> Create(ClubTask model)
    {
        var error = await ValidateSourceScope(model); if (error != null) return BadRequest(new ApiError(error));
        model.Id = 0; db.Tasks.Add(model); await db.SaveChangesAsync(); return Ok(new { model.Id });
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet("{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await CanManage(id)) return Forbid(); var x = await db.Tasks.FindAsync(id); if (x == null) return NotFound();
        return Ok(new TaskEditResponse(x, await LoadSources()));
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, ClubTask model)
    {
        if (id != model.Id) return BadRequest(); if (!await CanManage(id)) return Forbid();
        var error = await ValidateSourceScope(model); if (error != null) return BadRequest(new ApiError(error));
        var x = await db.Tasks.FindAsync(id); if (x == null) return NotFound();
        x.Title = model.Title; x.Description = model.Description; x.Priority = model.Priority; x.StartDate = model.StartDate; x.DueDate = model.DueDate; x.Status = model.Status; x.ActivityId = model.ActivityId; x.MeetingId = model.MeetingId; await db.SaveChangesAsync(); return NoContent();
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, AssignTaskRequest request)
    {
        var memberId = request.MemberId;
        if (!await CanManage(id) || !await scope.CanAccessMemberAsync(User, memberId)) return Forbid();
        if (await db.TaskAssignments.AnyAsync(x => x.TaskId == id && x.MemberId == memberId)) return BadRequest(new ApiError("Thành viên đã được phân công nhiệm vụ này."));
        db.TaskAssignments.Add(new() { TaskId = id, MemberId = memberId }); await db.SaveChangesAsync(); return NoContent();
    }

    [Authorize(Roles = AppRoles.Member), HttpPost("{id:int}/progress")]
    public async Task<IActionResult> Progress(int id, UpdateTaskProgressRequest request)
    {
        var m = await scope.MemberAsync(User); if (m == null) return Forbid();
        try { await service.UpdateTaskProgressAsync(id, m.Id, request.Progress); } catch (ArgumentOutOfRangeException ex) { return BadRequest(new ApiError(ex.Message)); }
        return NoContent();
    }

    private async Task<TaskSourcesResponse> LoadSources()
    {
        var activities = db.Activities.AsQueryable(); var meetings = db.Meetings.AsQueryable();
        if (User.IsInRole(AppRoles.ClubLeader)) { var m = await scope.MemberAsync(User); if (m == null) return new TaskSourcesResponse([], []); activities = activities.Where(x => x.ClubId == m.ClubId); meetings = meetings.Where(x => x.ClubId == m.ClubId); }
        var a = (await activities.OrderBy(x => x.ActivityName).ToListAsync()).Select(x => new TaskLookupItem(x.Id, x.ActivityName)).ToList();
        var mt = (await meetings.OrderBy(x => x.Title).ToListAsync()).Select(x => new TaskLookupItem(x.Id, x.Title)).ToList();
        return new TaskSourcesResponse(a, mt);
    }

    private async Task<string?> ValidateSourceScope(ClubTask model)
    {
        if (!User.IsInRole(AppRoles.ClubLeader)) return null;
        var m = await scope.MemberAsync(User);
        var activityAllowed = model.ActivityId != null && m != null && await db.Activities.AnyAsync(x => x.Id == model.ActivityId && x.ClubId == m.ClubId);
        var meetingAllowed = model.MeetingId != null && m != null && await db.Meetings.AnyAsync(x => x.Id == model.MeetingId && x.ClubId == m.ClubId);
        return !activityAllowed && !meetingAllowed ? "Trưởng câu lạc bộ phải chọn hoạt động hoặc cuộc họp thuộc câu lạc bộ của mình." : null;
    }

    private async Task<bool> CanManage(int taskId) { if (User.IsInRole(AppRoles.Admin)) return true; var m = await scope.MemberAsync(User); return m != null && await db.Tasks.AnyAsync(x => x.Id == taskId && ((x.Activity != null && x.Activity.ClubId == m.ClubId) || (x.Meeting != null && x.Meeting.ClubId == m.ClubId) || x.Assignments.Any(a => a.Member.ClubId == m.ClubId))); }
}
