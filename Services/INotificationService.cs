using JobSeeker.Models;
using JobSeeker.Models.Enums;

namespace JobSeeker.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string userId, string title, string message, string? linkUrl, NotificationType type);
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<Notification>> GetRecentNotificationsAsync(string userId, int count = 10);
        Task<List<Notification>> GetAllNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
