using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[Authorize]
public class ReportsController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index() => View(await api.GetAsync<ReportViewModel>("api/reports"));
}
