using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Api;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, ApplicationDbContext db, JwtTokenService tokens) : ControllerBase
{
    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await users.FindByNameAsync(model.Login) ?? await users.FindByEmailAsync(model.Login)
            ?? await db.Members.Where(x => x.StudentCode == model.Login && x.UserId != null).Select(x => x.User).SingleOrDefaultAsync();
        if (user != null && user.Status == DomainStatus.Active && (await signIn.CheckPasswordSignInAsync(user, model.Password, true)).Succeeded)
            return Ok(await tokens.CreateAsync(user));
        return Unauthorized(new ApiError("Thông tin đăng nhập không đúng hoặc tài khoản bị khóa."));
    }

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    [Authorize, HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
    {
        var user = await users.GetUserAsync(User); if (user is null) return Unauthorized();
        var result = await users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded) return BadRequest(new ApiError(string.Join(" ", result.Errors.Select(e => e.Description))));
        return NoContent();
    }
}

