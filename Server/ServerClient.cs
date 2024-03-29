﻿using System;
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
           // ThreadPool.QueueUserWorkItem((object o) =>
           // {
              //  Server.ReplicaState.ChangeState();
            //    Server.ReplicaState.InitReplica(Server.State.Profile, Server.State.Messages,
            //        Server.State.Contacts, Server.State.FriendRequests, Server.State.PendingInvitations, Server.State.Server_version);
           // });
        }

        #region IServerClient Members

        /// <summary>
        /// Write Operations
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Message Post(string message)
        {
            var m = Server.State.MakeMessage(message);
            try
            {
               Server.State.AddMessage(m);
                ThreadPool.QueueUserWorkItem((object o) => this.ServerServer.BroadCastMessage(m));
            }
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("Post Error:Service Not Available");
                throw;
            }
            return m;
            
        }

        public void PostFriendRequest(string address)
        {
            var c = new Contact();
            c.IP = address;
            try
            {
                Server.State.AddFriendRequest(c);
                ThreadPool.QueueUserWorkItem((object o) => ServerServer.SendFriendRequest(address));
            }
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("Error Friend Request:Service Not Available");
                throw;
            }
        }

        public Message RespondToFriendRequest(Contact c, bool accept)
        {
            var msg = new Message();
            if (accept)
            {
                var s = "YUPI!! I have a new friend: " + c.Username + "(" + c.IP.Trim() + ").";
                try
                {
                    Server.ReplicaState.CommitChanges();
                    msg = Server.State.MakeMessage(s);
                    ThreadPool.QueueUserWorkItem((object o) => NewFriendAnouncement(c, msg));
                }
                catch (ServiceNotAvailableException)
                {
                    Console.WriteLine("Error Friend Request:Service Not Available");
                    throw;
                }            
            }
            return msg;
        }

        public void NewFriendAnouncement(Contact c, Message msg) {
            ServerServer.SendFriendRequestConfirmation(Server.State.MakeContact(), c.IP.Trim());
            ServerServer.BroadCastMessage(msg);
            Server.State.AddMessage(msg);
            Server.State.AddContact(c);
            Server.State.RemoveFriendInvitation(c);
        }

        public void RefreshView()
        {
            try
            {
                Server.ReplicaState.CommitChanges();

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
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("Error RefreshView: Service Not Available");
                throw;
            }            
        }

        public void UpdateProfile(Profile profile)
        {
               try
                {
                    Console.WriteLine("Client: Update Profile");
                    Server.State.UpdateProfile(profile);
                }
                catch (ServiceNotAvailableException)
                {
                    Console.WriteLine("Error UpdateProfile:Service Not Available");
                    throw;
                }
        }

        /// <summary>
        /// Read Opertations
        /// </summary>
        /// <returns></returns>

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
            //Novo Master
            Server.ReplicaState.ChangeState();
            ConnectClient();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
        #endregion
        
        /// <summary>
        /// Searching Functions
        /// </summary>
        /// <param name="name"></param>
        public List<string> SCSearchByName(string s)
        {
            return ServerServer.SearchByName(s);
        }
        public List<string> SCSearchBySexAge(string s)
        {
            return ServerServer.SearchBySexAge(s);
        }
        public List<string> SCSearchByInterest(string s)
        {
            return ServerServer.SearchByInterest(s);
        }
    }
}
