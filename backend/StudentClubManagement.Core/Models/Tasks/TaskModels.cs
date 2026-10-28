using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class ClubTask
{
    public int Id { get; set; }
    public int? ActivityId { get; set; }
    public int? MeetingId { get; set; }
    [Required, MaxLength(180)] public string Title { get; set; } = "";
    [MaxLength(2000)] public string? Description { get; set; }
    [MaxLength(20)] public string Priority { get; set; } = "MEDIUM";
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "OPEN";
    [Range(0, 100)] public int Progress { get; set; }
    public Activity? Activity { get; set; }
    public Meeting? Meeting { get; set; }
    public ICollection<TaskAssignment> Assignments { get; set; } = [];
}

public class TaskAssignment
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int MemberId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(30)] public string CompletionStatus { get; set; } = "ASSIGNED";
    public DateTime? CompletedAt { get; set; }
    public ClubTask Task { get; set; } = null!;
    public Member Member { get; set; } = null!;
}

