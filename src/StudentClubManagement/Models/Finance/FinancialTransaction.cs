using System.ComponentModel.DataAnnotations;

namespace StudentClubManagement.Models;

public class FinancialTransaction
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    [Required, MaxLength(10)] public string TransactionType { get; set; } = "INCOME";
    [Required, MaxLength(100)] public string Category { get; set; } = "";
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal Amount { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public int CreatedById { get; set; }
    public Club Club { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;
}

