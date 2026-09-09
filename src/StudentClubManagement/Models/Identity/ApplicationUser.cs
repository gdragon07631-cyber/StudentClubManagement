using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudentClubManagement.Models;

public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(30)] public string Status { get; set; } = DomainStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Member? Member { get; set; }
}

