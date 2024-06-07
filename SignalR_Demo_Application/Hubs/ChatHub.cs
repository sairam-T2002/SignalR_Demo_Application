using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using SignalR_Demo_Application.App_Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SignalR_Demo_Application.Hubs
{
    // Interface for client-side methods
    public interface IChatClient
    {
        Task ReceiveMessage( string user, string message, bool IsGroup, string groupName );
        Task ActiveUsersList( List<User> activeUsers );
        Task SetMessages( List<Message> messages );
        Task MessageNotification( string frm );
    }

    // Hub class for SignalR
    public class ChatHub : Hub<IChatClient>
    {
        private readonly SQLiteHelper sqlite;

        public ChatHub( SQLiteHelper _sqlite )
        {
            sqlite = _sqlite;
        }
        public override async Task OnDisconnectedAsync( Exception exception )
        {
            if (exception != null)
            {
                Console.WriteLine($"Client disconnected with error: {exception.Message}");
            }
            else
            {
                Console.WriteLine("Client disconnected successfully.");
            }

            var connectionId = Context.ConnectionId;
            string query = $"DELETE FROM usrs WHERE conn_id = '{connectionId}';DELETE FROM msgs WHERE frm = '{connectionId}' OR too = '{connectionId}';SELECT * FROM usrs;";
            List<User> Active_Users = new List<User>();
            DataTable dataTable = sqlite.ExecuteQuery(query);
            foreach (DataRow usr in dataTable.Rows)
            {
                User temp = new User();
                temp.Username = usr["name"].ToString();
                temp.ConnectionId = usr["conn_id"].ToString();
                Active_Users.Add(temp);
            }
            await Clients.All.ActiveUsersList(Active_Users);
            await base.OnDisconnectedAsync(exception);
        }

        // Send a message to all clients
        public async Task SendMessage( string user, string message )
        {
            await Clients.All.ReceiveMessage(user, message, false, "");
        }

        // Authenticate and add a user
        public async Task Authenticate( string user, string ConnectionId )
        {
            string query = $"INSERT INTO usrs (name, conn_id,isgrp) VALUES ('{user.Replace("'", "''")}', '{ConnectionId}',0);SELECT * FROM usrs WHERE isgrp = 0;";
            List<User> Active_Users = new List<User>();
            DataTable dataTable = sqlite.ExecuteQuery(query);
            foreach (DataRow usr in dataTable.Rows)
            {
                User temp = new User();
                temp.Username = usr["name"].ToString();
                temp.ConnectionId = usr["conn_id"].ToString();
                temp.isgrp = usr["isgrp"].ToString() == "1";
                Active_Users.Add(temp);
            }
            await Clients.All.ActiveUsersList(Active_Users);
        }

        // Send a message to a specific client
        public async Task SendMessageToClient( string connectionIdFrom, string connectionIdTo, string user, string message )
        {
            string query = $"INSERT INTO msgs (frm,too,msg,isgrp) VALUES ('{connectionIdFrom}','{connectionIdTo}','{message.Replace("'", "''")}',0);";
            sqlite.ExecuteNonQuery(query);
            //await Clients.Client(connectionIdTo).ActiveUsersList(ActiveUsers.Active_Users);
            var connectionIds = new List<string> { connectionIdFrom, connectionIdTo };
            await Clients.Clients(connectionIdTo).MessageNotification(connectionIdFrom);
            await Clients.Clients(connectionIds).ReceiveMessage(user, message, false, "");
        }
        public async Task RetrieveMessages( string connectionIdFrom, string connectionIdTo, bool isGroup )
        {
            string query = "";
            if (isGroup)
            {
                query = @$"select u1.Name as [FROM],mgs.too as [TO],mgs.msg,mgs.isgrp FROM msgs mgs 
                           JOIN usrs u1 on mgs.frm = u1.conn_id where mgs.too = '{connectionIdTo}' and mgs.isgrp = 1;";
            }
            else
            {
                query = @$"SELECT u1.Name AS [from],u2.Name AS [to],mgs.msg,mgs.isgrp FROM msgs mgs JOIN usrs u1 ON 
                           mgs.frm = u1.conn_id JOIN usrs u2 ON mgs.too = u2.conn_id WHERE (mgs.frm = '{connectionIdFrom}' 
                           AND mgs.too = '{connectionIdTo}') || (mgs.too = '{connectionIdFrom}' 
                           AND mgs.frm = '{connectionIdTo}');";
            }
            List<Message> messages = new List<Message>();
            DataTable dataTable = sqlite.ExecuteQuery(query);
            foreach (DataRow msg in dataTable.Rows)
            {
                Message temp = new Message();
                temp.frm = msg["from"].ToString();
                temp.to = msg["to"].ToString();
                temp.msg = msg["msg"].ToString();
                temp.isgrp = msg["isgrp"].ToString() == "1";
                messages.Add(temp);
            }
            await Clients.Clients(connectionIdFrom).SetMessages(messages);
        }
        public async Task CreateGroup( string groupName, List<User> users )
        {
            foreach (var user in users)
            {
                await Groups.AddToGroupAsync(user.ConnectionId, groupName);
            }
            string query = $"INSERT INTO usrs (name, conn_id,isgrp) VALUES ('{groupName.Replace("'", "''")}', 'GROUP',1);SELECT * FROM usrs;";
            List<User> Active_Users = new List<User>();
            DataTable dataTable = sqlite.ExecuteQuery(query);
            foreach (DataRow usr in dataTable.Rows)
            {
                User temp = new User();
                temp.Username = usr["name"].ToString();
                temp.ConnectionId = usr["conn_id"].ToString();
                temp.isgrp = usr["isgrp"].ToString() == "1";
                Active_Users.Add(temp);
            }
            await Clients.Group(groupName).ActiveUsersList(Active_Users);
        }

        public async Task RemoveFromGroup( string groupName, List<string> connectionIds )
        {
            foreach (var connectionId in connectionIds)
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupName);
            }
        }

        public async Task SendMessageToGroup( string groupName, string user, string message )
        {
            string query = $"INSERT INTO msgs (frm,too,msg,isgrp) VALUES ('{Context.ConnectionId}','{groupName.Replace("'", "''")}','{message.Replace("'", "''")}',1);";
            sqlite.ExecuteNonQuery(query);
            await Clients.Group(groupName).MessageNotification(groupName);
            await Clients.Group(groupName).ReceiveMessage(user, message, true, groupName);
        }
    }
}
