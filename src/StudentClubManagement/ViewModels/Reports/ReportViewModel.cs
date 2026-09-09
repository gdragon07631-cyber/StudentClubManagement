using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

public record ReportViewModel(int Members, int Clubs, int Activities, int Tasks, decimal TotalPoints, decimal Income, decimal Expense);
