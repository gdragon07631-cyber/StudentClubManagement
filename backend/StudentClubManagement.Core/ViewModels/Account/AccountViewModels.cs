using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

public class LoginViewModel
{
    [Required, Display(Name = "Email hoặc MSSV")] public string Login { get; set; } = "";
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; } = "";
    [Display(Name = "Ghi nhớ đăng nhập")] public bool RememberMe { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password), Display(Name = "Mật khẩu mới")] public string NewPassword { get; set; } = "";
    [Compare(nameof(NewPassword)), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")] public string ConfirmPassword { get; set; } = "";
}

public class UserCreateViewModel
{
    [Required, MaxLength(50), Display(Name = "Tên đăng nhập")] public string Username { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password), Display(Name = "Mật khẩu tạm thời")] public string Password { get; set; } = "";
    [Required, Display(Name = "Vai trò")] public string Role { get; set; } = AppRoles.Member;
}

