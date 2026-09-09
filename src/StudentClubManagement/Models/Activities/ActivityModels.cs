using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class Activity
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    [Required, MaxLength(180)] public string ActivityName { get; set; } = "";
    [MaxLength(2000)] public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [MaxLength(300)] public string? Location { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "PLANNED";
    [MaxLength(2000)] public string? Result { get; set; }
    public Club Club { get; set; } = null!;
}

public class ActivityRegistration
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public int MemberId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    [MaxLength(30)] public string Status { get; set; } = "REGISTERED";
    public Activity Activity { get; set; } = null!;
    public Member Member { get; set; } = null!;
}

public class ActivityAttendance
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public int MemberId { get; set; }
    [Required, MaxLength(20)] public string AttendanceStatus { get; set; } = "PRESENT";
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public Activity Activity { get; set; } = null!;
    public Member Member { get; set; } = null!;
}

