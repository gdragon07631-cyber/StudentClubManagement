using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Authorization;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/dashboard"), Authorize]
public class DashboardController(ApplicationDbContext db, IAccessScopeService scope, IClubService clubs) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = await scope.UserIdAsync(User);
        var member = await scope.MemberAsync(User);
        if (!User.IsInRole(AppRoles.Admin) && member == null) return Forbid();
        var vm = new DashboardViewModel
        {
            DisplayName = member?.FullName ?? User.Identity?.Name ?? "Thành viên CLB",
            ClubName = member?.Club?.ClubName,
            UnreadNotifications = userId == null ? 0 : await db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead),
            UpcomingActivities = await db.Activities.Where(x => x.Status != "CANCELLED").OrderBy(x => x.StartTime).Take(3).ToListAsync(),
            UpcomingMeetings = await db.Meetings.Where(x => x.Status != "CANCELLED").OrderBy(x => x.MeetingTime).Take(3).ToListAsync(),
            RecentNotifications = userId == null ? [] : await db.Notifications.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Take(4).ToListAsync()
        };
        if (User.IsInRole(AppRoles.Admin))
        {
            vm.Members = await db.Members.CountAsync(); vm.Clubs = await db.Clubs.CountAsync();
            vm.Activities = await db.Activities.CountAsync(); vm.OpenTasks = await db.Tasks.CountAsync(x => x.Progress < 100);
            vm.Balance = await clubs.BalanceAsync();
            vm.ClubItems = await db.Clubs.OrderBy(x => x.ClubName).Select(x => new ClubSummaryViewModel(x.Id, x.ClubName, x.Description, db.Members.Count(m => m.ClubId == x.Id))).Take(4).ToListAsync();
        }
        else if (member != null)
        {
            var memberIds = User.IsInRole(AppRoles.ClubLeader)
                ? db.Members.Where(x => x.ClubId == member.ClubId).Select(x => x.Id)
                : db.Members.Where(x => x.Id == member.Id).Select(x => x.Id);
            vm.Members = await memberIds.CountAsync();
            var taskIds = db.TaskAssignments.Where(x => memberIds.Contains(x.MemberId)).Select(x => x.TaskId);
            vm.MyTasks = await db.Tasks.Include(x => x.Assignments).ThenInclude(x => x.Member).Where(x => taskIds.Contains(x.Id)).OrderBy(x => x.DueDate).Take(8).ToListAsync();
            vm.OpenTasks = vm.MyTasks.Count(x => x.Progress < 100); vm.Activities = await db.Activities.CountAsync(x => x.ClubId == member.ClubId);
            vm.Points = await db.PointHistory.Where(x => x.MemberId == member.Id).SumAsync(x => x.Points);
            vm.UpcomingActivities = await db.Activities.Where(x => x.ClubId == member.ClubId && x.Status != "CANCELLED").OrderBy(x => x.StartTime).Take(3).ToListAsync();
            vm.UpcomingMeetings = await db.Meetings.Where(x => x.ClubId == member.ClubId && x.Status != "CANCELLED").OrderBy(x => x.MeetingTime).Take(3).ToListAsync();
        }
        return Ok(vm);
    }
}
