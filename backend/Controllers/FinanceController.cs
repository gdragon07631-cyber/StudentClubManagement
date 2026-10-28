using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/finance"), Authorize(Roles = AppRoles.Admin)]
public class FinanceController(ApplicationDbContext db, IClubService service, UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? type, string sort = "newest")
    {
        var query = db.FinancialTransactions.AsQueryable(); if (from != null) query = query.Where(x => x.TransactionDate >= from); if (to != null) query = query.Where(x => x.TransactionDate <= to); if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.TransactionType == type);
        var balance = await service.BalanceAsync(from, to);
        query = sort == "amount" ? query.OrderByDescending(x => x.Amount) : query.OrderByDescending(x => x.TransactionDate);
        return Ok(new FinanceIndexResponse(await query.ToListAsync(), balance));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionRequest request)
    {
        if (request.Amount <= 0 || request.TransactionType is not ("INCOME" or "EXPENSE") || string.IsNullOrWhiteSpace(request.Category) || request.Category.Length > 100 || request.Description?.Length > 1000)
            return BadRequest(new ApiError("Giao dịch không hợp lệ; số tiền phải lớn hơn 0."));
        var model = new FinancialTransaction { TransactionType = request.TransactionType, Category = request.Category, Amount = request.Amount, Description = request.Description, TransactionDate = request.TransactionDate };
        model.ClubId = await db.Clubs.Select(x => x.Id).FirstAsync(); model.CreatedById = (await users.GetUserAsync(User))!.Id; db.FinancialTransactions.Add(model); await db.SaveChangesAsync();
        return NoContent();
    }
}
