using StudentClubManagement.Data;
using Microsoft.Extensions.Logging;

namespace StudentClubManagement.Services;

public class NotificationService(ApplicationDbContext db, ILogger<NotificationService> logger) : INotificationService
{
    public async Task CreateAsync(int userId, string title, string content, string type = "GENERAL") { db.Notifications.Add(new() { UserId = userId, Title = title, Content = content, NotificationType = type }); await db.SaveChangesAsync(); logger.LogInformation("Created in-app notification {Type} for user {UserId}; email adapter is not configured.", type, userId); }
}
