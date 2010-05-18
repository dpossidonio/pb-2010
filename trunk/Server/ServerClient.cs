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
        }

        private void Registration()
        {
            TcpChannel channel = new TcpChannel(int.Parse(Server.State.ServerIP.Split(':')[1]));
            ChannelServices.RegisterChannel(channel, false);

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
                Server.ReplicaState.InitReplica(Server.State.Profile, Server.State.Messages, Server.State.Contacts, Server.State.FriendRequests, Server.State.PendingInvitations);
            });
        }

        private void IsSystemDown() {
            //if (Server.State.ReplicationServers.Count == 0)
            //    throw new QuorumException(0, this);
        }

        #region IServerClient Members

        public Message Post(string message)
        {
            IsSystemDown();
            var m = Server.State.MakeMessage(message);

            Server.State.AddMessage(m);

            //Actualiza no profile o numero de sequencia dos seus posts
            Server.State.Profile = Server.State.Profile;

            ThreadPool.QueueUserWorkItem((object o) => this.ServerServer.BroadCastMessage(m));
            return m;
            
        }

        public void PostFriendRequest(string address)
        {
            IsSystemDown();
            var c = new Contact();
            c.IP = address;
            Server.State.AddFriendRequest(c);
            ThreadPool.QueueUserWorkItem((object o) => ServerServer.SendFriendRequest(address));
        }

        public Message RespondToFriendRequest(Contact c, bool accept)
        {
            IsSystemDown();
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
                    Server.State.RemoveFriendInvitation(c);
                });
            }
            return msg;
        }

        public void RefreshView()
        {
            IsSystemDown();
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
            IsSystemDown();
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

        public IList<Contact> GetPendingInvitations()
        {
            Console.WriteLine("Client: Get Pending Invitations");
            return Server.State.PendingInvitations;
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
