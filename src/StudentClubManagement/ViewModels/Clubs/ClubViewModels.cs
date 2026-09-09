using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.ViewModels;

public class ClubFormViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(160), Display(Name = "Tên câu lạc bộ")]
    public string ClubName { get; set; } = "";

    [MaxLength(1000), Display(Name = "Mô tả")]
    public string? Description { get; set; }
}

public record ClubListItemViewModel(
    int Id,
    string ClubName,
    string? Description,
    int MemberCount,
    int ActivityCount,
    int MeetingCount);
