using System.Collections.Generic;

namespace SignalR_Demo_Application
{
    public class User
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
        public List<string> Messages { get; set; }
        public Group Group { get; set; }
        public bool isgrp { get; set; }
        public int notification { get; set; }
    }
    public static class ActiveUsers
    {
        static ActiveUsers()
        {
            Active_Users = new List<User>();
        }
        public static List<User> Active_Users { get; private set; }
    }
    public class Group
    {
        public string GroupName { get; set; }
        public List<User> GroupMembers { get; set; }
    }
    public static class ActiveGroups
    {
        static ActiveGroups()
        {
            Active_Groups = new List<Group>();
        }
        public static List<Group> Active_Groups { get; set; }
    }
    public class ChatConnection
    {
        public User FromUser { get; set; }
        public User ToUser { get; set; }
    }
    public class Message
    {
        public string frm { get; set; }
        public string to { get; set; }
        public string msg { get; set; }
        public bool isgrp { get; set; }
    }
}
