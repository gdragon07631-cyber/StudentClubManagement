using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

public class AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, ApplicationDbContext db) : Controller
{
    [AllowAnonymous, HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByNameAsync(model.Login) ?? await users.FindByEmailAsync(model.Login)
            ?? await db.Members.Where(x => x.StudentCode == model.Login && x.UserId != null).Select(x => x.User).SingleOrDefaultAsync();
        if (user != null && user.Status == DomainStatus.Active && (await signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, true)).Succeeded)
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        ModelState.AddModelError("", "Thông tin đăng nhập không đúng hoặc tài khoản bị khóa."); return View(model);
    }
    [Authorize, HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction(nameof(Login)); }
    [Authorize, HttpGet] public IActionResult ChangePassword() => View();
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model); var user = await users.GetUserAsync(User); if (user is null) return Challenge();
        var result = await users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return View(model); }
        await signIn.RefreshSignInAsync(user); TempData["Success"] = "Đã đổi mật khẩu."; return RedirectToAction("Index", "Dashboard");
    }
    [AllowAnonymous] public IActionResult AccessDenied() => View();
}
