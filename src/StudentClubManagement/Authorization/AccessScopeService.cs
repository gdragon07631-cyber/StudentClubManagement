using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;

namespace StudentClubManagement.Authorization;

public interface IAccessScopeService { Task<int?> UserIdAsync(ClaimsPrincipal principal); Task<Member?> MemberAsync(ClaimsPrincipal principal); Task<bool> CanAccessMemberAsync(ClaimsPrincipal principal, int memberId); }

public class AccessScopeService(UserManager<ApplicationUser> users, ApplicationDbContext db) : IAccessScopeService
{
    public Task<int?> UserIdAsync(ClaimsPrincipal p) => Task.FromResult<int?>(int.TryParse(users.GetUserId(p), out var id) ? id : null);
    public async Task<Member?> MemberAsync(ClaimsPrincipal p) => int.TryParse(users.GetUserId(p), out var id) ? await db.Members.Include(x => x.Club).Include(x => x.Department).FirstOrDefaultAsync(x => x.UserId == id) : null;
    public async Task<bool> CanAccessMemberAsync(ClaimsPrincipal p, int memberId)
    {
        if (p.IsInRole(AppRoles.Admin)) return true;
        var current = await MemberAsync(p); if (current is null) return false;
        if (p.IsInRole(AppRoles.Member)) return current.Id == memberId;
        return p.IsInRole(AppRoles.ClubLeader) && await db.Members.AnyAsync(x => x.Id == memberId && x.ClubId == current.ClubId);
    }
}
