using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChapeauPOS.Hubs
{
    public class RestaurantHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var session = Context.GetHttpContext()?.Session;
            Employee employee = session.GetObject<Employee>("LoggedInUser");

            if (employee != null)
            {
                switch(employee.Role)
                {
                    case Roles.Cook:
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Cooks");
                        Console.WriteLine("Cook connected to RestaurantHub");
                        break;
                    case Roles.Waiter:
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Waiters");
                        Console.WriteLine("Waiter connected to RestaurantHub");
                        break;
                    case Roles.Bartender:
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Bartenders");
                        Console.WriteLine("Bartender connected to RestaurantHub");
                        break;
                    case Roles.Manager:
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");
                        Console.WriteLine("Manager connected to RestaurantHub");
                        break;
                }
            }
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Logic to handle when a client disconnects
            Console.WriteLine("Client disconnected from RestaurantHub");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
