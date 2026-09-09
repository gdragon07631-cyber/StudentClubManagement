using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

public record AiResultViewModel(string Function, string Result, bool IsFallback = false);

