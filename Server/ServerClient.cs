using System;
using System.Collections.Generic;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Threading;

namespace Server
{
    public class ServerClient : MarshalByRefObject, IServerClient
    {
        public ServerServer ServerServer;
        public IClient Client;

        public ServerClient()
        {
            ServerServer = new ServerServer(this);
            Registration();
            Server.State.DeserializeState();        
            //test();
        }

        private void Registration()
        {
            TcpChannel channel = new TcpChannel(int.Parse(Server.State.ServerIP.Split(':')[1]));
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(this, "IServerClient", typeof(IServerClient));
            RemotingServices.Marshal(this.ServerServer, "IServerServer", typeof(IServerServer));
        }

        private void ConnectClient()
        {
            Client = (IClient)Activator.GetObject(typeof(IClient),
                        string.Format("tcp://{0}/IClient", Server.State.Profile.IP));
            //REPLICAÇÂO -isto tb pode ir para a mesma classe ServerServer?
            ThreadPool.QueueUserWorkItem((object o) =>
            {
                Server.ReplicaState.ChangeState();
                Server.ReplicaState.InitReplica(Server.State.Profile, Server.State.Messages, Server.State.Contacts);
            });
        }
        //Metodo exclusivamente para testes
        public void test()
        {
            //Test
            var p = new Profile();

            p.UserName = "David";
            p.Age = 25;
            p.Interests.Add(Interest.Movies);
            p.Interests.Add(Interest.Science);
            p.Gender = Gender.Male;

            var ip = "127.0.0.1:123";
            for (int i = 0; i < 4; i++)
            {
                var c = new Contact();
                c.IP = ip + i;
                c.Username = "A" + i;
                Server.State.AddContact(c);
            }

            Server.State.UpdateProfile(p);

            for (int i = 0; i < 4; i++)
            {
                var msg = new Message();
                msg.FromUserName = "David";
                msg.Post = string.Format("Post#{0}", i);
                msg.Time = DateTime.Now;
                Server.State.AddMessage(msg);
            }

            //TextWriter tw = new StreamWriter("PADIdatabase.xml");
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(p.GetType());
            //x.Serialize(tw, p);
            //Console.WriteLine("object written to file");
            //Console.ReadLine();
            //tw.Close();
            //end Test
        }

        #region IServerClient Members

        public void Freeze() { }

        public Message Post(string message)
        {          
            var m = Server.State.MakeMessage(message);           
            //REPLICAÇÂO - DaVID- isto pode ir para a mesma classe ServerServer?
            //Server.ReplicaState.RegisterMessage(m);

            Server.State.AddMessage(m);

                //Actualiza no profile o numero de sequencia dos seus posts
                Server.State.UpdateProfile(Server.State.Profile);
            
            ThreadPool.QueueUserWorkItem((object o) => this.ServerServer.BroadCastMessage(m));
            return m;
        }

        public void PostFriendRequest(string address)
        {
            var c = new Contact();
            c.Username = Server.State.Profile.UserName;
            c.IP = Server.State.ServerIP;
            c.LastMsgSeqNumber = Server.State.Profile.PostSeqNumber;
            ThreadPool.QueueUserWorkItem((object o) => ServerServer.SendFriendRequest(c, address));
        }

        public Message RespondToFriendRequest(Contact c, bool accept)
        {
            var msg = new Message();
            if (accept)
            {
                var s = "YUPI!! I have a new friend: " + c.Username + "(" + c.IP.Trim() + ").";
                msg = Server.State.MakeMessage(s);

                ThreadPool.QueueUserWorkItem((object o) =>
                {
                    ServerServer.SendFriendRequestConfirmation(Server.State.MakeContact(), c.IP.Trim());
                    ServerServer.BroadCastMessage(msg);
                    Server.State.AddMessage(msg);
                    Server.State.AddContact(c);
                });
            }
            Server.State.FriendRequests.Remove(c);
            return msg;
        }

        public void RefreshView()
        {
            ThreadPool.QueueUserWorkItem((object o) =>
            {
                Console.WriteLine("Server: Refresh View");
                foreach (var item in Server.State.Contacts)
                {
                    var aux = ServerServer.SendRequestMessages(item.IP, item.LastMsgSeqNumber);
                    ServerServer.RefreshLocalMessages(aux, item);
                }
            });
        }

        public void UpdateProfile(Profile profile)
        {
            Console.WriteLine("Client: Update Profile");
            Server.State.UpdateProfile(profile);
        }

        public Profile GetProfile()
        {
            Console.WriteLine("Client: Get Profile");
            return Server.State.Profile;
        }

        public IList<Contact> GetFriendsContacts()
        {
            Console.WriteLine("Client: Get Friends");
            return Server.State.Contacts;
        }

        public IList<Contact> GetFriendsRequestsContacts()
        {
            Console.WriteLine("Client: Get Friends Requests");
            return Server.State.FriendRequests;
        }

        public IList<Message> GetMessages()
        {
            Console.WriteLine("Client: Messages");
            return Server.State.Messages;
        }

        public void Connect(string ip)
        {
            Console.WriteLine("Client: Connect IP:" + ip);
            Server.State.Profile.IP = ip;
            ConnectClient();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }


        #endregion
    }

}
