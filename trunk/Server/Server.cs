using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CommonTypes;

namespace Server
{
    public class Server
    {
        public static ServerState State;

        static void Main(string[] args)
        {
            State = new ServerState();

            Console.Write("Port:");
            var porto = Console.ReadLine();
            var ip = string.Format("127.0.0.1:{0}",porto);
            Console.WriteLine("Running Server on: "+ip);
            var sc = new ServerClient(ip);

            while (true) {
                //TODO . recebe comandos do utilizador Freeze()..
                Console.ReadLine();
            }
        }
    }

    public class ServerState
    {
        public Profile Profile { get; set; }
        public IList<Message> Messages { get;set;}
        public IList<Contact> Contacts { get; set; }
        public IList<Contact> FriendRequests { get; set; }

        public ServerState()
        {
            Profile = new Profile();
            Messages = new List<Message>();
            Contacts = new List<Contact>();
            FriendRequests = new List<Contact>();
        }

        //TODO - serializar a class
    }

}
