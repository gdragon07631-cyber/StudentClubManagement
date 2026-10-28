using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Authorization;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/members"), Authorize]
public class MembersController(ApplicationDbContext db, IAccessScopeService scope) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q, int? clubId, string? status, string sort = "name")
    {
        var current = await scope.MemberAsync(User); if (!User.IsInRole(AppRoles.Admin) && current == null) return Forbid(); var query = db.Members.Include(x => x.Club).AsQueryable();
        if (User.IsInRole(AppRoles.Member) && current != null) query = query.Where(x => x.Id == current.Id);
        else if (User.IsInRole(AppRoles.ClubLeader) && current != null) query = query.Where(x => x.ClubId == current.ClubId);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.FullName.Contains(q) || x.StudentCode.Contains(q));
        if (User.IsInRole(AppRoles.Admin) && clubId != null) query = query.Where(x => x.ClubId == clubId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.MemberStatus == status);
        query = sort switch { "studentCode" => query.OrderBy(x => x.StudentCode), "newest" => query.OrderByDescending(x => x.JoinedAt), _ => query.OrderBy(x => x.FullName) };
        return Ok(new MemberListResponse(await query.ToListAsync(), await ClubOptions()));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups() => Ok(new MemberFormLookups(await ClubOptions(), await UserOptions()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        if (!await scope.CanAccessMemberAsync(User, id)) return Forbid();
        var member = await db.Members.Include(x => x.Club).Include(x => x.Skills).SingleOrDefaultAsync(x => x.Id == id);
        return member == null ? NotFound() : Ok(member);
    }

    [Authorize(Roles = AppRoles.Admin), HttpPost]
    public async Task<IActionResult> Create(MemberCreateRequest model)
    {
        if (!await db.Clubs.AnyAsync(x => x.Id == model.ClubId)) return BadRequest(new ApiError("Vui lòng chọn câu lạc bộ hợp lệ."));
        if (model.UserId != null && await db.Members.AnyAsync(x => x.UserId == model.UserId)) return BadRequest(new ApiError("Tài khoản này đã được liên kết với thành viên khác."));
        var member = new Member
        {
            UserId = model.UserId, ClubId = model.ClubId, DepartmentId = null, StudentCode = model.StudentCode, FullName = model.FullName,
            ClassName = model.ClassName, Course = model.Course, Position = model.Position, Phone = model.Phone, Email = model.Email, MemberStatus = model.MemberStatus
        };
        db.Members.Add(member);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { return BadRequest(new ApiError("MSSV đã tồn tại trong câu lạc bộ.")); }
        return Ok(new { member.Id });
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await scope.CanAccessMemberAsync(User, id)) return Forbid(); var x = await db.Members.FindAsync(id); if (x == null) return NotFound();
        var vm = new MemberEditViewModel { Id = x.Id, UserId = x.UserId, FullName = x.FullName, Email = x.Email, Phone = x.Phone, ClassName = x.ClassName, Course = x.Course, Position = x.Position, ClubId = x.ClubId, MemberStatus = x.MemberStatus };
        return Ok(new MemberEditResponse(vm, await ClubOptions(), await UserOptions()));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, MemberEditViewModel model)
    {
        model.Id = id;
        if (!await scope.CanAccessMemberAsync(User, model.Id)) return Forbid(); var x = await db.Members.FindAsync(model.Id); if (x == null) return NotFound();
        if (User.IsInRole(AppRoles.Admin) && !await db.Clubs.AnyAsync(c => c.Id == model.ClubId)) return BadRequest(new ApiError("Vui lòng chọn câu lạc bộ hợp lệ."));
        if (User.IsInRole(AppRoles.Admin) && model.UserId != null && await db.Members.AnyAsync(m => m.Id != model.Id && m.UserId == model.UserId)) return BadRequest(new ApiError("Tài khoản này đã được liên kết với thành viên khác."));
        x.FullName = model.FullName; x.Email = model.Email; x.Phone = model.Phone; x.ClassName = model.ClassName; x.Course = model.Course;
        if (!User.IsInRole(AppRoles.Member)) x.Position = model.Position;
        if (User.IsInRole(AppRoles.Admin)) { x.UserId = model.UserId; x.ClubId = model.ClubId; x.DepartmentId = null; x.MemberStatus = model.MemberStatus; }
        await db.SaveChangesAsync(); return NoContent();
    }

    [Authorize(Roles = AppRoles.Admin), HttpPut("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, string status)
    {
        if (status is not (DomainStatus.Active or "INACTIVE")) return BadRequest(new ApiError("Trạng thái không hợp lệ.")); var x = await db.Members.FindAsync(id); if (x == null) return NotFound();
        x.MemberStatus = status; await db.SaveChangesAsync(); return NoContent();
        // TODO(BUSINESS_RULE_CONFIRMATION): dùng trạng thái thay vì xóa hồ sơ cho tới khi quy tắc rời CLB được chốt.
    }

    private async Task<List<ClubOption>> ClubOptions() => await db.Clubs.OrderBy(x => x.ClubName).Select(x => new ClubOption(x.Id, x.ClubName)).ToListAsync();
    private async Task<List<UserOption>> UserOptions() => await db.Users.OrderBy(x => x.UserName).Select(x => new UserOption(x.Id, x.UserName)).ToListAsync();
}
