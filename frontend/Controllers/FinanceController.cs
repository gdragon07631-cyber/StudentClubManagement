using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class FinanceController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? type, string sort = "newest")
    {
        var data = await api.GetAsync<FinanceIndexResponse>(ApiClient.Url("api/finance", ("from", from?.ToString("o")), ("to", to?.ToString("o")), ("type", type), ("sort", sort)));
        ViewBag.Balance = data.Balance; ViewBag.Type = type;
        return View(data.Transactions);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FinancialTransaction model)
    {
        ModelState.Remove(nameof(FinancialTransaction.Club)); ModelState.Remove(nameof(FinancialTransaction.CreatedBy));
        if (!ModelState.IsValid || model.Amount <= 0 || model.TransactionType is not ("INCOME" or "EXPENSE")) { TempData["Error"] = "Giao dịch không hợp lệ; số tiền phải lớn hơn 0."; return RedirectToAction(nameof(Index)); }
        var r = await api.PostAsync("api/finance", new CreateTransactionRequest { TransactionType = model.TransactionType, Category = model.Category, Amount = model.Amount, Description = model.Description, TransactionDate = model.TransactionDate });
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
}
