using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

public class MembershipApplicationViewModel : IValidatableObject
{
    [Required, Display(Name = "Năm sinh")] public int? BirthYear { get; set; }
    [Required, MaxLength(1000), Display(Name = "Sở thích")] public string Interests { get; set; } = "";
    [Required, MaxLength(2000), Display(Name = "Lý do chọn câu lạc bộ")] public string JoinReason { get; set; } = "";
    [Required, MaxLength(2000), Display(Name = "Mong muốn khi vào câu lạc bộ")] public string Expectations { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (BirthYear is not null && (BirthYear < 1900 || BirthYear > DateTime.Today.Year))
            yield return new ValidationResult("Năm sinh không hợp lệ.", [nameof(BirthYear)]);
    }
    [Range(1, int.MaxValue), Display(Name = "Câu lạc bộ")] public int ClubId { get; set; }
    [Required, MaxLength(30), Display(Name = "MSSV")] public string StudentCode { get; set; } = "";
    [Required, MaxLength(160), Display(Name = "Họ tên")] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Phone, Display(Name = "Điện thoại")] public string? Phone { get; set; }
    [Display(Name = "Lớp")] public string? ClassName { get; set; }
    [Display(Name = "Khóa")] public string? Course { get; set; }
}
