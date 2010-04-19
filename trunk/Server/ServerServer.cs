using System;
using System.Collections.Generic;
using CommonTypes;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.Linq;
using System.Threading;

namespace Server
{

    public class ServerServer : MarshalByRefObject, IServerServer
    {
        public ServerClient ServerClient;

        public ServerServer(ServerClient sc)
        {
            ServerClient = sc;
        }

        #region OUTBOUND

        #region Delegates & Asyncs

        private delegate void RemoteAsyncDelegateMessage(Message msg);
        private static void OurRemoteAsyncCallBackMessage(IAsyncResult ar)
        {
            RemoteAsyncDelegateMessage del = (RemoteAsyncDelegateMessage)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        //pode-se melhorar isto se der para passar qualquer coisa void* para o RemoteAsyncDelegate
        private delegate void RemoteAsyncDelegateContact(Contact c);
        private static void OurRemoteAsyncCallBackContact(IAsyncResult ar)
        {
            RemoteAsyncDelegateContact del = (RemoteAsyncDelegateContact)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        #endregion

        public void BroadCastMessage(Message msg)
        {
            Console.WriteLine("#Start BroadCasting Message with SeqNumber:" + msg.SeqNumber);
            foreach (var item in Server.State.Contacts)
            {
                //TODO: Servidor do Cliente??
                //var friend_server_ip = item.IP.Substring(0, item.IP.Length - 1) + "1";

                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer),
                string.Format("tcp://{0}/IServerServer", item.IP.Trim()));

                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackMessage);
                    RemoteAsyncDelegateMessage RemoteDel = new RemoteAsyncDelegateMessage(obj.ReceiveMessage);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(msg, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Server with the address {0} does not respond.", item.IP);

                }


            }
            Console.WriteLine("#End BroadCasting Message");

        }

        public void SendFriendRequest(Contact c, string IP)
        {
            Console.WriteLine("-->Sending Friend Request.");

            var obj = (IServerServer)Activator.GetObject(
               typeof(IServerServer),
               string.Format("tcp://{0}/IServerServer", IP));
            try
            {
                AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackContact);
                RemoteAsyncDelegateContact RemoteDel = new RemoteAsyncDelegateContact(obj.ReceiveFriendRequest);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(c, RemoteCallback, null);
            }
            catch (SocketException)
            {
                Console.WriteLine("-->The Server with the address {0} does not respond.", IP);
            }
        }

        public IList<Message> SendRequestMessages(string IP, int lastSeqNumber)
        {
            Console.WriteLine("-->Sending Request Messages to:{0} with SeqNumber > {1}", IP, lastSeqNumber);

            var obj = (IServerServer)Activator.GetObject(
               typeof(IServerServer),
               string.Format("tcp://{0}/IServerServer", IP));

            IList<Message> res = new List<Message>();
           
            try
            {
                res = (List<Message>)obj.RequestMessages(lastSeqNumber);
            }
            catch (SocketException)
            {
                Console.WriteLine("-->The Server with the address {0} does not respond.", IP);
            }
            return res;
        }

        public void SendFriendRequestConfirmation(Contact c, string IP)
        {
            Console.WriteLine("-->Sending Confirmation to Friend Request.");

            var obj = (IServerServer)Activator.GetObject(
                   typeof(IServerServer),
                   string.Format("tcp://{0}/IServerServer", IP));
            try
            {
                AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackContact);
                RemoteAsyncDelegateContact RemoteDel = new RemoteAsyncDelegateContact(obj.ReceiveFriendRequestOK);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(c, RemoteCallback, null);
            }
            catch (SocketException)
            {
                Console.WriteLine("-->The Server with the address {0} does not respond.", IP);
            }

        }

        public IList<Message> RequestMessages(int lastSeqNumber)
        {
            var res = Server.State.Messages.Where(x => x.FromUserName.Equals(Server.State.Profile.UserName) && x.SeqNumber > lastSeqNumber).ToList();
            Console.WriteLine("Server: Sending missing Messages.");
            return res;
        }

        #endregion

        #region INBOUND - IServerServer Members

        public void ReceiveFriendRequest(Contact c)
        {
            Console.WriteLine("<--Received FR from: " + c.Username + ",address: " + c.IP+", SeqNumber:"+c.LastMsgSeqNumber);
            lock (Server.State.FriendRequests)
            {
                Server.State.FriendRequests.Add(c);
                Server.State.SerializeObject(Server.State.Contacts);
            }
            var lc = new List<Contact>();
            lc.Add(c);

            //verifica se esta um cliente ligado, para actualizar a sua interface
            if (ServerClient.Client != null)
                ServerClient.Client.UpdateFriendRequest(lc);
        }
        public void ReceiveFriendRequestOK(Contact c)
        {
            Console.WriteLine("<--Received confirmation of a FR send to:" + c.Username + ",address:" + c.IP+",SeqNumber:"+c.LastMsgSeqNumber);

            ThreadPool.QueueUserWorkItem((object o) =>
            {
                var s = "YUPI!! I have a new friend: " + c.Username + "(" + c.IP.Trim() + ").";
                var msg = Server.State.MakeMessage(s);
                lock (Server.State.Messages)
                {
                    Server.State.Messages.Add(msg);
                }

                var lm = new List<Message>();
                lm.Add(msg);
                ServerClient.Client.UpdatePosts(lm);
                ServerClient.Client.UpdateFriends(c);
                BroadCastMessage(msg);

                lock (Server.State.Contacts)
                {
                    Server.State.Contacts.Add(c);
                    Server.State.SerializeObject(Server.State.Contacts);
                }
            });
        }

        public void ReceiveMessage(Message msg)
        {
            Console.WriteLine("<--Received post from:{0} SeqNumber:{1} Post:{2}", msg.FromUserName, msg.SeqNumber, msg.Post);

            Contact c = Server.State.Contacts.First(x => x.Username.Equals(msg.FromUserName));

            if (msg.SeqNumber == c.LastMsgSeqNumber + 1)
            {
                lock (Server.State.Messages)
                {
                    Server.State.Messages.Add(msg);
                    Server.State.SerializeObject(Server.State.Messages);
                }

                lock (Server.State.Contacts)
                {
                    c.LastMsgSeqNumber = msg.SeqNumber;
                    Server.State.SerializeObject(Server.State.Contacts);
                }
                var lm = new List<Message>();
                lm.Add(msg);

                try
                {
                    if (ServerClient.Client != null)
                        ServerClient.Client.UpdatePosts(lm);
                }
                catch (Exception)
                {
                    Console.WriteLine("Server: Client is not reachable!");
                }
            }
            else
            {
                var aux = SendRequestMessages(c.IP,c.LastMsgSeqNumber);
                RefreshLocalMessages(aux, c);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        public void RefreshLocalMessages(IList<Message> msgs,Contact c)
        {
            if (msgs != null && msgs.Count > 0)
            {
                lock (Server.State.Messages)
                {
                    foreach (var item in msgs)
                    {
                        Server.State.Messages.Add(item);
                        Server.State.SerializeObject(Server.State.Messages);
                    }
                }
                lock (Server.State.Contacts)
                {
                    c.LastMsgSeqNumber = msgs.Max(x => x.SeqNumber);
                    Server.State.SerializeObject(Server.State.Contacts);

                }
                //pedreiro
                if(ServerClient.Client != null)
                ServerClient.Client.UpdatePosts(msgs);
            }
        }
    }
}
