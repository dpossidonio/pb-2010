using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        public static ServerState State;

        static void Main(string[] args)
        {
            Console.WriteLine("Port:");
            var porto = Console.ReadLine();
            var ip = string.Format("127.0.0.1:{0}",porto);
            Console.WriteLine("Running Server on: "+ip);
            State = new ServerState();
            var sc = new ServerClient(ip);

            while (true) {
                Console.ReadLine();
            }
        }
    }

    public class ServerState
    {
        public Profile Profile { get; set; }
        public IList<Message> Messages { get; set; }
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

    public class ServerClient : MarshalByRefObject, IServerClient
    {
        public ServerServer ServerServer;

        public ServerClient(string ip)
        {
            ServerServer = new ServerServer();
            Registration(ip);
            test();
        }

        private void Registration(string ip)
        {
            TcpChannel channel = new TcpChannel(int.Parse(ip.Split(':')[1]));
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(this,"IServerClient",typeof(IServerClient));
            RemotingServices.Marshal(this.ServerServer, "IServerServer", typeof(IServerServer));
        }

        public void test()
        {
            //Test
            var p = new Profile();

            p.UserName = "David";
            p.Age = 25;
            p.Interests.Add(Interest.Movies);
            p.Interests.Add(Interest.Science);
            p.Gender = Gender.Male;
            var c = new Contact();
            c.IP = "127.0.0.1:1234";
            Server.State.Contacts.Add(c);

            Server.State.Profile = p;
            //TextWriter tw = new StreamWriter("PADIdatabase.xml");
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(p.GetType());
            //x.Serialize(tw, p);
            //Console.WriteLine("object written to file");
            //Console.ReadLine();
            //tw.Close();
            //end Test
        }

        #region IServerClient Members

        public IList<Message> Connect(string ClientIP)
        {
            var res = new List<Message>();
            var msg = new Message();
            msg.FromUserName = "David";
            msg.Post = "Ola sou eu outra vez desculpa o incómodo.";
            msg.Time = DateTime.Now;
            res.Add(msg);

            return res;
        }

        public IList<Contact> GetContacts()
        {
            throw new NotImplementedException();
        }

        public void Freeze()
        {
            throw new NotImplementedException();
        }

        public void Post(string message)
        {
            var m = new Message();
            m.FromUserName = "Xoxas";
            m.Post = message;
            ServerServer.Send(m);
        }

        public void FriendRequest(string address)
        {

        }

        public void RespondToFriendRequest(string username, bool accept)
        {
            throw new NotImplementedException();
        }

        public IList<Message> RefreshView()
        {
            throw new NotImplementedException();
        }

        public void UpdateProfile(Profile profile)
        {
            Server.State.Profile = profile;
        }

        public Profile GetProfile()
        {
            return Server.State.Profile;
        }

        #endregion
    }

    public class ServerServer : MarshalByRefObject, IServerServer
    {
        #region OUTBOUND

        public delegate void RemoteAsyncDelegate(Message msg);
        public static void OurRemoteAsyncCallBack(IAsyncResult ar)
        {
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("Recebi confirmação de um servidor");
            return;
        }

        public void Send(Message msg)
        {
            foreach (var item in Server.State.Contacts)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer),
                string.Format("tcp://{0}/IServerServer", item.IP));

                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBack);
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.ReceiveMessage);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(msg, RemoteCallback, null);
                }
                catch (SocketException) { }
            }
        }

        #endregion

        #region INBOUND - IServerServer Members

        public void ReceiveFriendRequest(Contact Request)
        {
            Server.State.FriendRequests.Add(Request);
            //TODO
        }

        public void ReceiveMessage(Message msg)
        {
            Console.WriteLine("Recebi:" + msg.Post + "\r\n");
            lock (Server.State.Messages)
            {
                Server.State.Messages.Add(msg);
            }
        }

        #endregion
    }
}
