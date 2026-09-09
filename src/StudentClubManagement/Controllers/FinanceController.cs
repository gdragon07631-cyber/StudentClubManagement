using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class FinanceController(ApplicationDbContext db, IClubService service, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? type, string sort = "newest") { var query = db.FinancialTransactions.AsQueryable(); if (from != null) query = query.Where(x => x.TransactionDate >= from); if (to != null) query = query.Where(x => x.TransactionDate <= to); if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.TransactionType == type); ViewBag.Balance = await service.BalanceAsync(from, to); ViewBag.Type = type; query = sort == "amount" ? query.OrderByDescending(x => x.Amount) : query.OrderByDescending(x => x.TransactionDate); return View(await query.ToListAsync()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FinancialTransaction model) { ModelState.Remove(nameof(FinancialTransaction.Club)); ModelState.Remove(nameof(FinancialTransaction.CreatedBy)); if (!ModelState.IsValid || model.Amount <= 0 || model.TransactionType is not ("INCOME" or "EXPENSE")) { TempData["Error"] = "Giao dịch không hợp lệ; số tiền phải lớn hơn 0."; return RedirectToAction(nameof(Index)); } model.ClubId = await db.Clubs.Select(x => x.Id).FirstAsync(); model.CreatedById = (await users.GetUserAsync(User))!.Id; db.FinancialTransactions.Add(model); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}

