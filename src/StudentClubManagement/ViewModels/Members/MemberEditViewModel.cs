using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

public class MemberEditViewModel
{
    public int Id { get; set; }
    [Display(Name = "Tài khoản đăng nhập")] public int? UserId { get; set; }
    [Required, MaxLength(160), Display(Name = "Họ tên")] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Phone, Display(Name = "Điện thoại")] public string? Phone { get; set; }
    [Display(Name = "Lớp")] public string? ClassName { get; set; }
    [Display(Name = "Khóa")] public string? Course { get; set; }
    [Display(Name = "Chức vụ")] public string? Position { get; set; }
    [Display(Name = "Câu lạc bộ")] public int ClubId { get; set; }
    [Display(Name = "Trạng thái")] public string MemberStatus { get; set; } = DomainStatus.Active;
}
