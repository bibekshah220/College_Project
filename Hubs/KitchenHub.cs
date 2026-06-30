using Microsoft.AspNetCore.SignalR;

namespace CafeManagementSystem.Hubs
{
    public class KitchenHub : Hub
    {
        // Called by waiter when a new order is placed
        public async Task NewOrderPlaced(int orderId, string tableNumber)
        {
            await Clients.Group("Kitchen").SendAsync("ReceiveNewOrder", orderId, tableNumber);
        }

        // Called by kitchen when order status changes
        public async Task UpdateOrderStatus(int orderId, string status)
        {
            await Clients.All.SendAsync("OrderStatusUpdated", orderId, status);
        }

        // Join a group (e.g., "Kitchen" for kitchen staff)
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Leave a group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
