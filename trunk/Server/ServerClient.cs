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

            Server.State.Contacts = (IList<Contact>)Server.State.DeserializeObject(Server.State.Contacts);
            Server.State.Messages = (IList<Message>)Server.State.DeserializeObject(Server.State.Messages);
            Server.State.Profile = (Profile)Server.State.DeserializeObject(Server.State.Profile);

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
                Server.State.Contacts.Add(c);
            }

            Server.State.Profile = p;

            for (int i = 0; i < 4; i++)
            {
                var msg = new Message();
                msg.FromUserName = "David";
                msg.Post = string.Format("Post#{0}", i);
                msg.Time = DateTime.Now;

                lock (Server.State.Messages)
                {
                    Server.State.Messages.Add(msg);
                }
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
            lock (Server.State.Messages)
            {
                Server.State.Messages.Add(m);
                //Serializa as mensagens
                Server.State.SerializeObject(Server.State.Messages);
                //Actualiza no profile o numero de sequencia dos seus posts
                Server.State.SerializeObject(Server.State.Profile);
            }
           ThreadPool.QueueUserWorkItem((object o) => this.ServerServer.BroadCastMessage(m));
            return m;
        }

        public void PostFriendRequest(string address)
        {
            //o username é desnecessário, só com o endereço do server dá para fazer o pedido

            var c = new Contact();
            c.Username = Server.State.Profile.UserName;
            c.IP = Server.State.ServerIP;
            //c.LastMsgSeqNumber ??
            ThreadPool.QueueUserWorkItem((object o) => ServerServer.SendFriendRequest(c, address));
        }

        public Message RespondToFriendRequest(Contact c, bool accept)
        {
            Message msg = new Message();
            //adicionar amigo
            if (accept)
            {
                var s = "YUPI!! I have a new friend: " + c.Username + "(" + c.IP.Trim() + ").";        
                //ATENÇÃO É NECESSÁRIO VER SE ESTA É A MELHOR ORDEM DE FAZER ISTO
                //difundir nova amizade por todos os amigos
                msg = Server.State.MakeMessage(s);

                ThreadPool.QueueUserWorkItem((object o) =>
                {
                    ServerServer.SendFriendRequestConfirmation(Server.State.MakeContact(), c.IP.Trim());

                    lock(Server.State.Messages){
                        Server.State.Messages.Add(msg);
                    }
                    ServerServer.BroadCastMessage(Server.State.MakeMessage(s));

                    //adicionar novo amigo aos contactos e informa-o da aceitação 
                    Server.State.Contacts.Add(c);
                    //serializa os contactos
                       Server.State.SerializeObject(Server.State.Contacts);
                });
            }

            Server.State.FriendRequests.Remove(c);
            return msg;
        }

        public IList<Message> RefreshView() { return null; }

        public void UpdateProfile(Profile profile)
        {
            Console.WriteLine("Client: Update Profile");
            Server.State.SerializeObject(profile);
            Server.State.Profile = profile;
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

        #endregion
    }

}
