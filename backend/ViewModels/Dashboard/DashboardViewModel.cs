using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

public class DashboardViewModel
{
    public string DisplayName { get; set; } = "";
    public string? ClubName { get; set; }
    public int Members { get; set; }
    public int Clubs { get; set; }
    public int Activities { get; set; }
    public int OpenTasks { get; set; }
    public int UnreadNotifications { get; set; }
    public decimal Points { get; set; }
    public decimal Balance { get; set; }
    public IEnumerable<ClubTask> MyTasks { get; set; } = [];
    public IEnumerable<ClubSummaryViewModel> ClubItems { get; set; } = [];
    public IEnumerable<Activity> UpcomingActivities { get; set; } = [];
    public IEnumerable<Meeting> UpcomingMeetings { get; set; } = [];
    public IEnumerable<Notification> RecentNotifications { get; set; } = [];
}

public record ClubSummaryViewModel(int Id, string ClubName, string? Description, int MemberCount);
