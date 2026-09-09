using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class Meeting
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    [Required, MaxLength(180)] public string Title { get; set; } = "";
    public DateTime MeetingTime { get; set; }
    [MaxLength(300)] public string? Location { get; set; }
    [MaxLength(3000)] public string? Agenda { get; set; }
    [MaxLength(6000)] public string? Minutes { get; set; }
    [MaxLength(3000)] public string? Decisions { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "SCHEDULED";
    public Club Club { get; set; } = null!;
}

public class MeetingAttendance
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int MemberId { get; set; }
    [Required, MaxLength(20)] public string AttendanceStatus { get; set; } = "PRESENT";
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public Meeting Meeting { get; set; } = null!;
    public Member Member { get; set; } = null!;
}

