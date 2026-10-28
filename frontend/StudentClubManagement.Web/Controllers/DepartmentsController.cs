using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Models;

namespace StudentClubManagement.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class DepartmentsController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Clubs");
    public IActionResult Create() => RedirectToAction("Create", "Clubs");
    public IActionResult Edit(int id) => RedirectToAction("Index", "Clubs");
}
