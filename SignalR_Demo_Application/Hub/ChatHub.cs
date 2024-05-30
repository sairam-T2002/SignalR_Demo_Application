using Microsoft.AspNetCore.SignalR;

namespace SignalR_Demo_Application.Hub
{
    public interface IChatClient
    {
        Task ReceiveMessage( string user, string message );
        Task AddUser( string user );
    }

    public class ChatHub : Hub<IChatClient>
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.ConnectionId != null)
            {
                Console.WriteLine($"New client connected with ID: {Context.ConnectionId}");
                ActiveUsers.Active_Users.Add(Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync( Exception exception )
        {
            if (exception != null)
            {
                // Handle disconnection error (optional)
                Console.WriteLine($"Client disconnected with error: {exception.Message}");
            }
            else
            {
                // Client disconnected gracefully
                Console.WriteLine("Client disconnected successfully.");
            }
            ActiveUsers.Active_Users.Remove(Context.ConnectionId);
        }
        public async Task SendMessage( string user, string message )
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        public async Task Authenticate( string user )
        {
            await Clients.All.AddUser(user);
        }
    }
}
