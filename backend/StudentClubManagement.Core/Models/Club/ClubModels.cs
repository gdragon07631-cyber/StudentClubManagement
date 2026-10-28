using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class Club
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string ClubName { get; set; } = "";
    [MaxLength(1000)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Department> Departments { get; set; } = [];
}

public class Department
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    [Required, MaxLength(120)] public string DepartmentName { get; set; } = "";
    [MaxLength(1000)] public string? Description { get; set; }
    public int? LeaderMemberId { get; set; }
    public Club Club { get; set; } = null!;
    public Member? LeaderMember { get; set; }
    public ICollection<Member> Members { get; set; } = [];
}

public class Member
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ClubId { get; set; }
    public int? DepartmentId { get; set; }
    [Required, MaxLength(30)] public string StudentCode { get; set; } = "";
    [Required, MaxLength(160)] public string FullName { get; set; } = "";
    [MaxLength(60)] public string? ClassName { get; set; }
    [MaxLength(30)] public string? Course { get; set; }
    [MaxLength(80)] public string? Position { get; set; }
    [MaxLength(30)] public string? Phone { get; set; }
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = "";
    [MaxLength(30)] public string MemberStatus { get; set; } = DomainStatus.Active;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser? User { get; set; }
    public Club Club { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<MemberSkill> Skills { get; set; } = [];
}

public class MembershipApplication
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    [Required, MaxLength(30)] public string StudentCode { get; set; } = "";
    [Required, MaxLength(160)] public string FullName { get; set; } = "";
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = "";
    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(60)] public string? ClassName { get; set; }
    [MaxLength(30)] public string? Course { get; set; }
    public int? BirthYear { get; set; }
    [MaxLength(1000)] public string? Interests { get; set; }
    [MaxLength(2000)] public string? JoinReason { get; set; }
    [MaxLength(2000)] public string? Expectations { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(30)] public string Status { get; set; } = DomainStatus.Pending;
    public int? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    [MaxLength(500)] public string? RejectionReason { get; set; }
    public Club Club { get; set; } = null!;
    public ApplicationUser? ReviewedBy { get; set; }
}

public class MemberSkill
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    [Required, MaxLength(100)] public string SkillName { get; set; } = "";
    [MaxLength(40)] public string? SkillLevel { get; set; }
    [MaxLength(1000)] public string? Experience { get; set; }
    public Member Member { get; set; } = null!;
}
