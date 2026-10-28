using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

/// <summary>Danh sách giao dịch kèm số dư.</summary>
public record FinanceIndexResponse(List<FinancialTransaction> Transactions, decimal Balance);

/// <summary>Yêu cầu tạo giao dịch tài chính.</summary>
public class CreateTransactionRequest
{
    public string TransactionType { get; set; } = "INCOME";
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
}
