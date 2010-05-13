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

        //pode-se melhorar isto se der para passar qualquer coisa void*(o VOID* em liguagem OO é Object) para o RemoteAsyncDelegate
        private delegate void RemoteAsyncDelegateContact(Contact c);
        private static void OurRemoteAsyncCallBackContact(IAsyncResult ar)
        {
            RemoteAsyncDelegateContact del = (RemoteAsyncDelegateContact)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        private delegate void RemoteAsyncDelegateUpdateContact(Contact c, bool b);
        private static void OurRemoteAsyncCallBackUpdateContact(IAsyncResult ar)
        {
            RemoteAsyncDelegateUpdateContact del = (RemoteAsyncDelegateUpdateContact)((AsyncResult)ar).AsyncDelegate;
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

        #endregion

        public void ReplicateMessage(List<string> destinations, Message msg)
        {
            Console.WriteLine("#REP: Message with SeqNumber:" + msg.SeqNumber);
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
                }
            }
            Console.WriteLine("#End REP");
        }

        public void SetSlave(List<string> replicas, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi)
        {
            Console.WriteLine("#Start Replicas Setup ");
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
                }
            }
            Console.WriteLine("#End Replicas Setup");
        }

        public void SetProfile(List<string> replicas, CommonTypes.Profile p)
        {
            Console.WriteLine("#REP: Profile");
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
                }
            }
            Console.WriteLine("#End REP Profile");
        }

        public void SetContact(List<string> replicas, CommonTypes.Contact c, bool updateSeqNumber)
        {
            Console.WriteLine("#REP: Contact");
            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateUpdateContact RemoteDel = new RemoteAsyncDelegateUpdateContact(obj.UpdateContacts);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c, updateSeqNumber, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                }
            }
            Console.WriteLine("#End REP Contact");
        }

        public void SetFriendRequest(List<string> replicas, CommonTypes.Contact c, bool b)
        {
            Console.WriteLine("#REP: Friend Request");
            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateUpdateContact RemoteDel = new RemoteAsyncDelegateUpdateContact(obj.UpdateFriendRequest);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c, b, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                }
            }
            Console.WriteLine("#End REP Friend Request");
        }

        public void SetFriendInvitation(List<string> replicas, CommonTypes.Contact c, bool b)
        {
            Console.WriteLine("#REP: FriendInvitation");
            foreach (var item in replicas)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer), string.Format("tcp://{0}/IServerServer", item));
                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackAll);
                    RemoteAsyncDelegateUpdateContact RemoteDel = new RemoteAsyncDelegateUpdateContact(obj.UpdatePendingInvitation);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c, b, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                }
            }
            Console.WriteLine("#End REP FriendInvitation");
        }

        /// <summary>
        /// INBOUND
        /// </summary>

        public void UpdateSlave(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi)
        {
            Console.WriteLine("<--#START FULL Updating Slave");
            Server.State.UpdateProfile(p);
            Server.State.Messages = m;
            Server.State.Contacts = c;
            Server.State.FriendRequests = fr;
            Server.State.PendingInvitations = pi;
            Console.WriteLine("<--#END FULL Updating Slave");
        }

        public void UpdateMessages(Message msg)
        {
            Server.State.AddMessage(msg);
            if (msg.FromUserName == Server.State.Profile.UserName)
                Server.State.Profile.PostSeqNumber = msg.SeqNumber;
            else //Se der excepção aki é porque os Contactos ainda n foram actualizados
                Server.State.Contacts.First(x => x.Username.Equals(msg.FromUserName)).LastMsgSeqNumber = msg.SeqNumber;
        }

        public void UpdateContacts(Contact c, bool updateSeqNumber)
        {
            if (updateSeqNumber) Server.State.UpdateSeqNumber(c, c.LastMsgSeqNumber);
            else Server.State.AddContact(c);
        }

        public void UpdateProfile(Profile p)
        {
            Server.State.UpdateProfile(p);
        }

        public void UpdateFriendRequest(Contact c, bool b)
        {
            if (b) Server.State.AddFriendRequest(c);
            else Server.State.RemoveFriendRequest(c);
        }

        public void UpdatePendingInvitation(Contact c, bool b)
        {
            if (b) Server.State.AddFriendInvitation(c);
            else Server.State.RemoveFriendInvitation(c);
        }

    }
}
