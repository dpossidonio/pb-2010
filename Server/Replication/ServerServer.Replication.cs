using System;
using System.Collections.Generic;
using System.Linq;
using CommonTypes;
using System.Runtime.Remoting.Messaging;

namespace Server
{
    public partial class ServerServer : MarshalByRefObject, IServerServer
    {

        /// <summary>
        /// OUTBOUND
        /// </summary>

        #region Delegates & Asyncs

        private delegate void RemoteAsyncDelegateMessage(Message msg);
        private static void OurRemoteAsyncCallBackMessage(IAsyncResult ar)
        {
            RemoteAsyncDelegateMessage del = (RemoteAsyncDelegateMessage)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        private delegate void RemoteAsyncDelegateProfile(Profile profile);
        private static void OurRemoteAsyncCallBackProfile(IAsyncResult ar)
        {
            RemoteAsyncDelegateProfile del = (RemoteAsyncDelegateProfile)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        private delegate void RemoteAsyncDelegateContact(Contact c);
        private static void OurRemoteAsyncCallBackContact(IAsyncResult ar)
        {
            RemoteAsyncDelegateContact del = (RemoteAsyncDelegateContact)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        private delegate void RemoteAsyncDelegateUpdateContact(Contact c);
        private static void OurRemoteAsyncCallBackUpdateContact(IAsyncResult ar)
        {
            RemoteAsyncDelegateUpdateContact del = (RemoteAsyncDelegateUpdateContact)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }


        private delegate void RemoteAsyncDelegateUpdateFriendRequest(Contact c,bool request);
        private static void OurRemoteAsyncCallBackUpdateFriendRequest(IAsyncResult ar)
        {
            RemoteAsyncDelegateUpdateFriendRequest del = (RemoteAsyncDelegateUpdateFriendRequest)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }


        private delegate void RemoteAsyncDelegateAll(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi);
        private static void OurRemoteAsyncCallBackAll(IAsyncResult ar)
        {
            RemoteAsyncDelegateAll del = (RemoteAsyncDelegateAll)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        private delegate void RemoteAsyncDelegateInitQuorum(string ip);
        private static void OurRemoteAsyncDelegateInitQuorum(IAsyncResult ar)
        {
            RemoteAsyncDelegateInitQuorum del = (RemoteAsyncDelegateInitQuorum)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        #endregion

        public void PingReplicationServers()
        {
            Console.WriteLine("#REP: Ping Replicated Servers");
            var failed_servers = new List<string>();
            foreach (var item in Server.State.ReplicationServers)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));

                try
                {
                    obj.Ping();
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    failed_servers.Add(item);  
                }
            }
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End REP");
        }

        public void InitReplication(List<string> destinations)
        {
            Console.WriteLine("#REP: Looking for Replicated Servers");
            var failed_servers = new List<string>();
            foreach (var item in destinations)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));

                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncDelegateInitQuorum);
                    RemoteAsyncDelegateInitQuorum RemoteDel = new RemoteAsyncDelegateInitQuorum(obj.StatusRequest);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(Server.State.ServerIP, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    failed_servers.Add(item);
                }
            }
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End REP");
        }

        public void ReplicateMessage(List<string> destinations, Message msg)
        {
            Console.WriteLine("#REP: Message with SeqNumber:" + msg.SeqNumber);
            var failed_servers = new List<string>();
            foreach (var item in destinations)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));

                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackMessage);
                    RemoteAsyncDelegateMessage RemoteDel = new RemoteAsyncDelegateMessage(obj.UpdateMessages);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(msg, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    failed_servers.Add(item);
                }
            }
            UpdateAvailableServers(failed_servers);
            if (Server.State.ReplicationServers.Count >= 1)
            {
                Console.WriteLine("#Message Commited!");
                Server.State.CommitMessage(msg);
            }
            else {
                Console.WriteLine("### Service Unnavailable ###");           
            }

            Console.WriteLine("#End REP");
        }

        public void SetSlave(List<string> replicas, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi)
        {
            Console.WriteLine("#Start Replicas Setup ");
            var failed_servers = new List<string>();
            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateAll RemoteDel = new RemoteAsyncDelegateAll(obj.UpdateSlave);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(p, m, c, fr, pi, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    failed_servers.Add(item);
                }
            }
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End Replicas Setup");
        }

        public void SetProfile(List<string> replicas, CommonTypes.Profile p)
        {
            Console.WriteLine("#REP: Profile");
            var failed_servers = new List<string>();

            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateProfile RemoteDel = new RemoteAsyncDelegateProfile(obj.UpdateProfile);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(p, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    failed_servers.Add(item);                                     
                }
            }
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End REP Profile");
        }

        public void SetContact(List<string> replicas, CommonTypes.Contact c)
        {
            Console.WriteLine("#REP: Contact");
            var failed_servers = new List<string>();
            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateUpdateContact RemoteDel = new RemoteAsyncDelegateUpdateContact(obj.UpdateContacts);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    failed_servers.Add(item);
                }
            }
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End REP Contact");
        }

        public void SetFriendRequest(List<string> replicas, CommonTypes.Contact c, bool b)
        {
            Console.WriteLine("#REP: Friend Request");
            var failed_servers = new List<string>();

            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateUpdateFriendRequest RemoteDel = new RemoteAsyncDelegateUpdateFriendRequest(obj.UpdateFriendRequest);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c, b, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Server.State.ReplicationServers.Remove(item);
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    failed_servers.Add(item);
                }
            }
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End REP Friend Request");
        }

        public void SetFriendInvitation(List<string> replicas, CommonTypes.Contact c, bool b)
        {
            Console.WriteLine("#REP: FriendInvitation");
            var failed_servers = new List<string>();

            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateUpdateFriendRequest RemoteDel = new RemoteAsyncDelegateUpdateFriendRequest(obj.UpdatePendingInvitation);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c, b, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                    Server.State.ReplicationServers.Remove(item);
                    failed_servers.Add(item);
                }
            }
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End REP FriendInvitation");
        }

        /// <summary>
        /// INBOUND
        /// </summary>

        public void Ping() {}
        public void StatusRequest(string ip) { }

        public void UpdateSlave(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi)
        {
            Console.WriteLine("<--#START FULL Updating State from Master");
            Server.State.CommitProfile(p);
            Server.State.Messages = m;
            Server.State.Contacts = c;
            Server.State.FriendRequests = fr;
            Server.State.PendingInvitations = pi;
            Console.WriteLine("<--#END FULL Updating State from Master");
        }

        public void UpdateMessages(Message msg)
        {
            Server.State.VerifyFreeze();
            Server.State.AddMessage(msg);
        }

        public void UpdateContacts(Contact c)
        {
            Server.State.VerifyFreeze();
            Server.State.AddContact(c);
        }

        public void UpdateProfile(Profile p)
        {
            Server.State.VerifyFreeze();
            Server.State.UpdateProfile(p);
        }

        public void UpdateFriendRequest(Contact c, bool b)
        {
            Server.State.VerifyFreeze();
            Console.WriteLine("SLAVE: Adding/Updating FriendRequest.");
            if (b) Server.State.CommitAddFriendRequest(c);
            else Server.State.CommitRemoveFriendRequest(c);
        }

        public void UpdatePendingInvitation(Contact c, bool b)
        {
            Server.State.VerifyFreeze();
            Console.WriteLine("SLAVE: Adding/Updating PendingInvitation.");
            if (b) Server.State.CommitAddFriendInvitation(c);
            else Server.State.CommitRemoveFriendInvitation(c);
        }

        /// <summary>
        ///  UTIL
        /// </summary>
        /// <param name="failed_servers"></param>
        private void UpdateAvailableServers(List<string> failed_servers){       
        Server.State.ReplicationServers = Server.State.ReplicationServers.Except(failed_servers).ToList();
        }
    }
}
