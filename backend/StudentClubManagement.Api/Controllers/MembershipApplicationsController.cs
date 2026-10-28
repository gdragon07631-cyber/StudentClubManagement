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

[ApiController, Route("api/membership-applications")]
public class MembershipApplicationsController(ApplicationDbContext db, IClubService service, UserManager<ApplicationUser> users, IAccessScopeService scope, AccountProvisioningService accounts) : ControllerBase
{
    private const string ManagerRoles = AppRoles.Admin + "," + AppRoles.ClubLeader;

    [AllowAnonymous, HttpGet("clubs")]
    public async Task<IActionResult> Clubs() => Ok(await db.Clubs.OrderBy(x => x.ClubName).Select(x => new ClubOption(x.Id, x.ClubName)).ToListAsync());

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> Apply(MembershipApplicationViewModel vm)
    {
        if (!await db.Clubs.AnyAsync(x => x.Id == vm.ClubId)) return BadRequest(new ApiError("Vui lòng chọn câu lạc bộ hợp lệ."));
        db.MembershipApplications.Add(new() { ClubId = vm.ClubId, StudentCode = vm.StudentCode, FullName = vm.FullName, Email = vm.Email, Phone = vm.Phone, ClassName = vm.ClassName, Course = vm.Course, BirthYear = vm.BirthYear, Interests = vm.Interests, JoinReason = vm.JoinReason, Expectations = vm.Expectations });
        await db.SaveChangesAsync(); return NoContent();
    }

    [AllowAnonymous, HttpGet("status")]
    public async Task<IActionResult> Status(string? code) => Ok(new ApplicationStatusResponse(await db.MembershipApplications.Where(x => x.StudentCode == code).OrderByDescending(x => x.AppliedAt).FirstOrDefaultAsync()));

    [Authorize(Roles = ManagerRoles), HttpGet]
    public async Task<IActionResult> Index(string? q, int? clubId, string? status, string sort = "newest")
    {
        var query = db.MembershipApplications.Include(x => x.Club).AsQueryable();
        if (User.IsInRole(AppRoles.ClubLeader)) { var current = await scope.MemberAsync(User); if (current == null) return Forbid(); query = query.Where(x => x.ClubId == current.ClubId); }
        else if (clubId != null) query = query.Where(x => x.ClubId == clubId);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.FullName.Contains(q) || x.StudentCode.Contains(q) || x.Email.Contains(q));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = sort == "name" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.AppliedAt);
        var clubs = await db.Clubs.OrderBy(x => x.ClubName).Select(x => new ClubOption(x.Id, x.ClubName)).ToListAsync();
        return Ok(new ApplicationListResponse(await query.ToListAsync(), clubs));
    }

    [Authorize(Roles = ManagerRoles), HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!await CanManage(id)) return Forbid();
        var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException();
        await service.ApproveApplicationAsync(id, u.Id); return NoContent();
    }

    [Authorize(Roles = ManagerRoles), HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, RejectApplicationRequest model)
    {
        if (!await CanManage(id)) return Forbid();
        if (string.IsNullOrWhiteSpace(model.Reason)) return BadRequest(new ApiError("Cần nhập lý do từ chối."));
        var u = await users.GetUserAsync(User) ?? throw new UnauthorizedAccessException();
        await service.RejectApplicationAsync(id, u.Id, model.Reason); return NoContent();
    }

    [Authorize(Roles = ManagerRoles), HttpGet("{id:int}/issue-account")]
    public async Task<IActionResult> IssueAccount(int id)
    {
        if (!await CanManage(id)) return Forbid();
        var application = await db.MembershipApplications.FindAsync(id);
        if (application == null) return NotFound();
        if (application.Status != DomainStatus.Approved) return BadRequest(new ApiError("Chỉ cấp tài khoản sau khi phê duyệt."));
        var member = await db.Members.Include(x => x.User).SingleOrDefaultAsync(x => x.ClubId == application.ClubId && x.StudentCode == application.StudentCode);
        if (member == null) return NotFound();
        return Ok(new IssueAccountViewModel { Id = id, FullName = member.FullName, Username = member.StudentCode, ExistingUsername = member.User?.UserName });
    }

    [Authorize(Roles = ManagerRoles), HttpPost("{id:int}/issue-account")]
    public async Task<IActionResult> IssueAccount(int id, IssueAccountViewModel model)
    {
        model.Id = id;
        if (!await CanManage(model.Id)) return Forbid();
        var application = await db.MembershipApplications.FindAsync(model.Id);
        if (application == null) return NotFound();
        if (application.Status != DomainStatus.Approved) return BadRequest(new ApiError("Chỉ cấp tài khoản sau khi phê duyệt."));
        var member = await db.Members.SingleOrDefaultAsync(x => x.ClubId == application.ClubId && x.StudentCode == application.StudentCode);
        if (member == null) return NotFound();
        if (member.UserId != null) return BadRequest(new ApiError("Sinh viên đã có tài khoản."));
        try { await accounts.IssueAsync(model.Id, model.Username, model.Password); }
        catch (InvalidOperationException ex) { return BadRequest(new ApiError(ex.Message)); }
        return NoContent();
    }

    private async Task<bool> CanManage(int id) { if (User.IsInRole(AppRoles.Admin)) return true; var current = await scope.MemberAsync(User); return current != null && await db.MembershipApplications.AnyAsync(x => x.Id == id && x.ClubId == current.ClubId); }
}
