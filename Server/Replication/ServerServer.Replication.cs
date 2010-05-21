using System;
using System.Collections.Generic;
using System.Linq;
using CommonTypes;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Server
{
    public partial class ServerServer : MarshalByRefObject, IServerServer
    {

        public void StartReplication() {
            //só deve ser um servidor que ele conhece do grupo
            //
            try
            {
                string int_server = Server.State.ReplicationServers.First();
                //fala com um (este pode não ser o master)
                InitReplication(Server.State.ReplicationServers);
                //fala com outros se houver
                InitReplication(Server.State.ReplicationServers.Except(new List<string>() { int_server }).ToList());
            }
            catch (InvalidOperationException) {
                Console.WriteLine("#StartReplication: No Serveres where found.");
            }
        }

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


        private delegate void RemoteAsyncDelegateAll(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi, long server_versionId);
        private static void OurRemoteAsyncCallBackAll(IAsyncResult ar)
        {
            RemoteAsyncDelegateAll del = (RemoteAsyncDelegateAll)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        //private delegate List<string> RemoteAsyncDelegateInitQuorum(string ip);
        //private static List<string> OurRemoteAsyncDelegateInitQuorum(IAsyncResult ar)
        //{
        //    RemoteAsyncDelegateInitQuorum del = (RemoteAsyncDelegateInitQuorum)((AsyncResult)ar).AsyncDelegate;
        //    del.EndInvoke(ar);
        //    return;
        //}

        #endregion


        /// <summary>
        /// OUTBOUND
        /// </summary>
        
        #region OUTBOUND Replication Members
       
        public void InitReplication(IList<string> servers)
        {
            Console.WriteLine("#REP: Looking for Replicated Servers");
            var failed_servers = new List<string>();
            foreach (var item in servers)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));

                try
                {
                    Server.State.ReplicationServers = Server.State.ReplicationServers.Union(obj.StatusRequest(Server.State.ServerIP)).ToList();
                    Console.WriteLine("%%%%FOUND SERVER:"+item);
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

        public void ReplicateMessage(Message msg)
        {
            Console.WriteLine("#REP: Message with SeqNumber:" + msg.SeqNumber);
            var failed_servers = new List<string>();
            foreach (var item in Server.State.ReplicationServers)
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
            UpdateMessages(msg);
            Console.WriteLine("#End REP");
        }

        public void SetSlave(string replica_addr)
        {
            Console.WriteLine("#Start FULL Update Replica");
            var failed_servers = new List<string>();
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", replica_addr));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateAll RemoteDel = new RemoteAsyncDelegateAll(obj.UpdateSlave);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(Server.State.Profile, Server.State.Messages, Server.State.Contacts, 
                        Server.State.FriendRequests, Server.State.PendingInvitations,Server.State.Server_version,
                        RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", replica_addr);
                    failed_servers.Add(replica_addr);
                }
            
            UpdateAvailableServers(failed_servers);
            Console.WriteLine("#End Replicas Setup");
        }

        public void SetProfile(CommonTypes.Profile p)
        {
            Console.WriteLine("#REP: Profile");
            var failed_servers = new List<string>();

            foreach (var item in Server.State.ReplicationServers)
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
            UpdateProfile(p);
            Console.WriteLine("#End REP Profile");
        }

        public void SetContact(CommonTypes.Contact c)
        {
            Console.WriteLine("#REP: Contact");
            var failed_servers = new List<string>();
            foreach (var item in Server.State.ReplicationServers)
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
            UpdateContacts(c);
            Console.WriteLine("#End REP Contact");
        }

        public void SetFriendRequest(CommonTypes.Contact c, bool b)
        {
            Console.WriteLine("#REP: Friend Request");
            var failed_servers = new List<string>();

            foreach (var item in Server.State.ReplicationServers)
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
            UpdateFriendRequest(c, b);
            Console.WriteLine("#End REP Friend Request");
        }

        public void SetFriendInvitation(CommonTypes.Contact c, bool b)
        {
            Console.WriteLine("#REP: FriendInvitation");
            var failed_servers = new List<string>();

            foreach (var item in Server.State.ReplicationServers)
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
            UpdatePendingInvitation(c, b);

            Console.WriteLine("#End REP FriendInvitation");
        }

        #endregion OUTBOUND Replication Members

        /// <summary>
        /// INBOUND
        /// </summary>

        #region INBOUND Replication Members

        public IList<string> StatusRequest(string addr) {
            Server.State.ReplicationServers = Server.State.ReplicationServers.Union(new List<string>() { addr }).ToList();

            ThreadPool.QueueUserWorkItem((object o)=> Server.ReplicaState.InitReplica(addr));

            return Server.State.ReplicationServers.Except(new List<string>() { addr }).ToList();
        }

        public void UpdateSlave(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, 
            IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi, long server_version_id)
        {
            Console.WriteLine("My Version-: {0} New Master Version: {1}", Server.State.Server_version,server_version_id);
     
            if (server_version_id > Server.State.Server_version)
            {
                Server.ReplicaState.State = new SlaveState();
                Console.WriteLine("<--#START FULL Updating State from Master");
                Server.State.CommitProfile(p);
                Server.State.Messages = m;
                Server.State.Contacts = c;
                Server.State.FriendRequests = fr;
                Server.State.PendingInvitations = pi;
                Server.State.Server_version = server_version_id;
                Console.WriteLine("<--#END FULL Updating State from Master");
            }
            else Console.WriteLine("It's not necessary to update!");
        }

        public void UpdateMessages(Message msg)
        {
            Server.State.VerifyFreeze();
            try
            {
                Server.State.CommitMessage(msg);

                Console.WriteLine("#Message Commited!");
            }
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("#Message NOT Commited!");
                throw;
            }
        }

        public void UpdateContacts(Contact c)
        {
            Server.State.VerifyFreeze();

            try
            {
                Server.State.CommitContact(c);
                Console.WriteLine("#Contact Commited!");
            }
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("#Contact NOT Commited!");
                throw;
            }
        }

        public void UpdateProfile(Profile p)
        {
           // Server.State.VerifyFreeze();
            try
            {
                Server.State.CommitProfile(p);
                Console.WriteLine("#Profile Commited!");
            }
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("#Profile NOT Commited!");
                throw;
            }
        }

        public void UpdateFriendRequest(Contact c, bool b)
        {
            Server.State.VerifyFreeze();

            try
            {
                if (b) Server.State.CommitAddFriendRequest(c);
                else Server.State.CommitRemoveFriendRequest(c);
                Console.WriteLine("#FriendRequest Commited!");
            }
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("#FriendRequest NOT Commited!");
                throw;
            }
        }

        public void UpdatePendingInvitation(Contact c, bool b)
        {
            Server.State.VerifyFreeze();
            try
            {
                if (b) Server.State.CommitAddFriendInvitation(c);
                else Server.State.CommitRemoveFriendInvitation(c);
                Console.WriteLine("#FriendEnvitation Commited!");
            }
            catch (ServiceNotAvailableException)
            {
                Console.WriteLine("#FriendEnvitation NOT Commited");
                throw;
            }
        }

        #endregion INBOUND Replication Members

        /// <summary>
        ///  UTIL
        /// </summary>
        /// <param name="failed_servers"></param>
        private void UpdateAvailableServers(List<string> failed_servers){       
        Server.State.ReplicationServers = Server.State.ReplicationServers.Except(failed_servers).ToList();
        if (Server.State.ReplicationServers.Count < 1)
            Server.ReplicaState.State = new UnnavailableState();
        }
    }
}
