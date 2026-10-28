using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class AIController(ApiClient api) : Controller
{
    [HttpGet] public IActionResult Chat() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Chat(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) { ModelState.AddModelError("", "Vui lòng nhập câu hỏi."); return View(); }
        var r = await api.PostAsync<AiResultViewModel>("api/ai/chat", new AiChatRequest(question));
        if (!r.Success) { r.AddTo(ModelState); return View(); }
        return View(r.Data);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Recommend(int taskId)
    {
        var r = await api.PostAsync<List<AiRecommendation>>($"api/ai/recommend/{taskId}");
        if (!r.Success) { TempData["Error"] = r.Error; return RedirectToAction("Index", "ClubTasks"); }
        ViewBag.TaskId = taskId; return View("Recommendations", r.Data ?? []);
    }
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.ClubLeader), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Participation(int memberId)
    {
        var r = await api.PostAsync<AiResultViewModel>($"api/ai/participation/{memberId}");
        if (!r.Success) { TempData["Error"] = r.Error; return View("Chat"); }
        return View("Chat", r.Data);
    }
}
