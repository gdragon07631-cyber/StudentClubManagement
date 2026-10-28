using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

/// <summary>Mục chọn câu lạc bộ cho dropdown.</summary>
public record ClubOption(int Id, string ClubName);

/// <summary>Mục chọn tài khoản cho dropdown.</summary>
public record UserOption(int Id, string? UserName);

/// <summary>Danh sách thành viên kèm danh sách câu lạc bộ để lọc.</summary>
public record MemberListResponse(List<Member> Items, List<ClubOption> Clubs);

/// <summary>Dữ liệu dropdown cho form thành viên.</summary>
public record MemberFormLookups(List<ClubOption> Clubs, List<UserOption> Users);

/// <summary>Dữ liệu form sửa thành viên.</summary>
public record MemberEditResponse(MemberEditViewModel Member, List<ClubOption> Clubs, List<UserOption> Users);

/// <summary>Yêu cầu tạo thành viên mới (Admin).</summary>
public class MemberCreateRequest
{
    public int? UserId { get; set; }
    public int ClubId { get; set; }
    [Required, MaxLength(30)] public string StudentCode { get; set; } = "";
    [Required, MaxLength(160)] public string FullName { get; set; } = "";
    [MaxLength(60)] public string? ClassName { get; set; }
    [MaxLength(30)] public string? Course { get; set; }
    [MaxLength(80)] public string? Position { get; set; }
    [MaxLength(30), Phone] public string? Phone { get; set; }
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = "";
    [MaxLength(30)] public string MemberStatus { get; set; } = DomainStatus.Active;
}
