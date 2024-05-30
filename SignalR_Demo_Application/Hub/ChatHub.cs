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
