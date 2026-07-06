using Microsoft.AspNetCore.SignalR;

namespace JobSeeker.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        public static async Task SendNotificationToUser(IHubContext<NotificationHub> hubContext, string userId, object notification)
        {
            await hubContext.Clients.Group(userId)
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
