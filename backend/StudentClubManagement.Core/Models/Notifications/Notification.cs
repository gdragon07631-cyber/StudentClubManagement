using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    [Required, MaxLength(180)] public string Title { get; set; } = "";
    [Required, MaxLength(2000)] public string Content { get; set; } = "";
    [MaxLength(50)] public string NotificationType { get; set; } = "GENERAL";
    [MaxLength(20)] public string Channel { get; set; } = "IN_APP";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser User { get; set; } = null!;
}

