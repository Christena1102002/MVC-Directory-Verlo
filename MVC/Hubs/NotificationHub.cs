using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace mvc.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        public async Task SendReviewNotification(int businessId, string message)
        {
            await Clients.All.SendAsync("ReceiveReviewNotification", businessId, message);
        }

        public async Task SendUserNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveUserNotification", message);
        }

        public async Task SendGroupNotification(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveGroupNotification", message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
