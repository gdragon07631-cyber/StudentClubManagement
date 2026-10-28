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

[ApiController, Route("api/meetings"), Authorize]
public class MeetingsController(ApplicationDbContext db, IClubService service, IAccessScopeService scope) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? status, string sort = "newest")
    {
        var current = await scope.MemberAsync(User); var query = db.Meetings.AsNoTracking().AsQueryable();
        if (!User.IsInRole(AppRoles.Admin)) { if (current == null) return Forbid(); query = query.Where(x => x.ClubId == current.ClubId); }
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Title.Contains(q) || (x.Location != null && x.Location.Contains(q)));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = sort == "title" ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.MeetingTime);
        var meetings = await query.ToListAsync();
        var cards = meetings.Select(m => new MeetingCard { Meeting = m }).ToList();
        if (!User.IsInRole(AppRoles.Member) && meetings.Count > 0)
        {
            var ids = meetings.Select(m => m.Id).ToList();
            var clubIds = meetings.Select(m => m.ClubId).Distinct().ToList();
            var attendances = await db.MeetingAttendance.AsNoTracking().Include(a => a.Member).Where(a => ids.Contains(a.MeetingId)).ToListAsync();
            var members = await db.Members.AsNoTracking().Where(m => clubIds.Contains(m.ClubId) && m.MemberStatus == DomainStatus.Active).OrderBy(m => m.FullName).ToListAsync();
            foreach (var card in cards)
            {
                card.Attendances = attendances.Where(a => a.MeetingId == card.Meeting.Id).ToList();
                var attendedIds = card.Attendances.Select(a => a.MemberId).ToHashSet();
                card.Unrecorded = members.Where(m => m.ClubId == card.Meeting.ClubId && !attendedIds.Contains(m.Id)).ToList();
            }
        }
        return Ok(cards);
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet("clubs")]
    public async Task<IActionResult> Clubs() => Ok(await db.Clubs.AsNoTracking().OrderBy(x => x.ClubName).Select(x => new ScheduleClubOption { Id = x.Id, ClubName = x.ClubName }).ToListAsync());

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var x = await db.Meetings.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid(); return Ok(x);
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost]
    public async Task<IActionResult> Create(MeetingInput input)
    {
        var model = ToEntity(input);
        var error = await SetAndValidateClub(model); if (error != null) return BadRequest(new ApiError(error));
        db.Meetings.Add(model); await db.SaveChangesAsync(); return Ok(new { model.Id });
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, MeetingInput input)
    {
        var x = await db.Meetings.FindAsync(id); if (x == null) return NotFound(); if (!await CanManage(x.ClubId)) return Forbid();
        var model = ToEntity(input);
        var error = await SetAndValidateClub(model); if (error != null) return BadRequest(new ApiError(error));
        x.ClubId = model.ClubId; x.Title = model.Title; x.MeetingTime = model.MeetingTime; x.Location = model.Location; x.Agenda = model.Agenda; x.Minutes = model.Minutes; x.Decisions = model.Decisions; x.Status = model.Status;
        await db.SaveChangesAsync(); return NoContent();
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost("{id:int}/attendance")]
    public async Task<IActionResult> Attendance(int id, AttendanceInput input)
    {
        var meeting = await db.Meetings.FindAsync(id); if (meeting == null) return NotFound();
        if (!await CanManage(meeting.ClubId) || !await scope.CanAccessMemberAsync(User, input.MemberId)) return Forbid();
        try { await service.RecordMeetingAttendanceAsync(id, input.MemberId, input.Status); return NoContent(); } catch (InvalidOperationException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    static Meeting ToEntity(MeetingInput i) => new() { ClubId = i.ClubId, Title = i.Title, MeetingTime = i.MeetingTime, Location = i.Location, Agenda = i.Agenda, Minutes = i.Minutes, Decisions = i.Decisions, Status = i.Status ?? "SCHEDULED" };
    private async Task<string?> SetAndValidateClub(Meeting model)
    {
        if (User.IsInRole(AppRoles.ClubLeader)) { var current = await scope.MemberAsync(User); if (current == null) return "Tài khoản chưa có hồ sơ sinh viên."; model.ClubId = current.ClubId; }
        if (!await db.Clubs.AnyAsync(x => x.Id == model.ClubId)) return "Vui lòng chọn câu lạc bộ hợp lệ.";
        return null;
    }
    private async Task<bool> CanManage(int clubId) { if (User.IsInRole(AppRoles.Admin)) return true; var current = await scope.MemberAsync(User); return current != null && current.ClubId == clubId; }
}
