using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

public class AccountController(ApiClient api) : Controller
{
    /// <summary>Phản hồi của POST api/auth/login.</summary>
    record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, int UserId, string UserName, string? Email, string[] Roles);

    [AllowAnonymous, HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.PostAsync<LoginResponse>("api/auth/login", new { model.Login, model.Password });
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError("", result.StatusCode == 401 || result.StatusCode == 400 ? (result.Error ?? "Thông tin đăng nhập không đúng hoặc tài khoản bị khóa.") : "Không kết nối được máy chủ API. Vui lòng thử lại sau.");
            return View(model);
        }
        var d = result.Data;
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, d.UserId.ToString()), new(ClaimTypes.Name, d.UserName), new(ApiClient.TokenClaim, d.AccessToken) };
        if (!string.IsNullOrEmpty(d.Email)) claims.Add(new Claim(ClaimTypes.Email, d.Email));
        claims.AddRange(d.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = model.RememberMe, ExpiresUtc = d.ExpiresAtUtc });
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return RedirectToAction(nameof(Login)); }

    [Authorize, HttpGet] public IActionResult ChangePassword() => View();

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.PostAsync("api/auth/change-password", new { model.CurrentPassword, model.NewPassword });
        if (!result.Success) { result.AddTo(ModelState); return View(model); }
        TempData["Success"] = "Đã đổi mật khẩu.";
        return RedirectToAction("Index", "Dashboard");
    }

    [AllowAnonymous] public IActionResult AccessDenied() => View();
}
