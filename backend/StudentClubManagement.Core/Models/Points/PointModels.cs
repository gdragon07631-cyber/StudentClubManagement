using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class PointRule
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string RuleName { get; set; } = "";
    [Required, MaxLength(30)] public string SourceType { get; set; } = "";
    public decimal PointValue { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    [MaxLength(30)] public string Status { get; set; } = DomainStatus.Active;
}

public class PointHistory
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int PointRuleId { get; set; }
    [Required, MaxLength(30)] public string SourceType { get; set; } = "";
    public int SourceId { get; set; }
    public decimal Points { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = "";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public Member Member { get; set; } = null!;
    public PointRule PointRule { get; set; } = null!;
}

