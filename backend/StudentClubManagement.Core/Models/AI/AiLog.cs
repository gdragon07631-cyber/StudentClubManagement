using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class AiLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    [Required, MaxLength(60)] public string AiFunction { get; set; } = "";
    [MaxLength(1000)] public string? InputSummary { get; set; }
    [MaxLength(6000)] public string? OutputResult { get; set; }
    [MaxLength(100)] public string? ModelName { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "SUCCESS";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser User { get; set; } = null!;
}

