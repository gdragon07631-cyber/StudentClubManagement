using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class PointsController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(string sort = "newest")
    {
        var data = await api.GetAsync<PointIndexResponse>(ApiClient.Url("api/points", ("sort", sort)));
        ViewBag.Sort = sort; return View(data);
    }
    [Authorize(Roles = AppRoles.Admin), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(PointHistory model)
    {
        ModelState.Remove(nameof(PointHistory.Member)); ModelState.Remove(nameof(PointHistory.PointRule));
        if (!ModelState.IsValid) { TempData["Error"] = "Dữ liệu điểm không hợp lệ."; return RedirectToAction(nameof(Index)); }
        var r = await api.PostAsync("api/points", new PointRecordInput { MemberId = model.MemberId, PointRuleId = model.PointRuleId, SourceType = model.SourceType, SourceId = model.SourceId, Points = model.Points, Reason = model.Reason });
        if (!r.Success) TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
}
