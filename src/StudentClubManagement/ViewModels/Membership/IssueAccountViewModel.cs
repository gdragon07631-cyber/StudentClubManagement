using System.ComponentModel.DataAnnotations;
namespace StudentClubManagement.ViewModels;
public class IssueAccountViewModel
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? ExistingUsername { get; set; }
    [Required, MaxLength(256), Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 8), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")]
    public string ConfirmPassword { get; set; } = "";
}
