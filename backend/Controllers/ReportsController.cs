using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Authorization;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/reports"), Authorize]
public class ReportsController(ApplicationDbContext db, IAccessScopeService scope) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(AppRoles.Member)) return Forbid();
        var current = await scope.MemberAsync(User);
        var members = db.Members.AsQueryable();
        var activities = db.Activities.AsQueryable();
        var tasks = db.Tasks.AsQueryable();
        var clubCount = await db.Clubs.CountAsync();
        if (User.IsInRole(AppRoles.ClubLeader))
        {
            if (current == null) return Forbid();
            members = members.Where(x => x.ClubId == current.ClubId);
            activities = activities.Where(x => x.ClubId == current.ClubId);
            tasks = tasks.Where(x => (x.Activity != null && x.Activity.ClubId == current.ClubId) || (x.Meeting != null && x.Meeting.ClubId == current.ClubId) || x.Assignments.Any(a => a.Member.ClubId == current.ClubId));
            clubCount = 1;
        }
        var memberIds = members.Select(x => x.Id);
        var income = User.IsInRole(AppRoles.Admin) ? await db.FinancialTransactions.Where(x => x.TransactionType == "INCOME").SumAsync(x => x.Amount) : 0;
        var expense = User.IsInRole(AppRoles.Admin) ? await db.FinancialTransactions.Where(x => x.TransactionType == "EXPENSE").SumAsync(x => x.Amount) : 0;
        return Ok(new ReportViewModel(await members.CountAsync(), clubCount, await activities.CountAsync(), await tasks.CountAsync(), await db.PointHistory.Where(x => memberIds.Contains(x.MemberId)).SumAsync(x => x.Points), income, expense));
    }
}
