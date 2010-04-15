using System;
using System.Collections.Generic;
using CommonTypes;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
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

        public void SendMessage(Message msg)
        {
            Console.WriteLine("#Start Sending Messages");
            foreach (var item in Server.State.Contacts)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer),
                string.Format("tcp://{0}/IServerServer", item.IP.Trim()));

                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackMessage);
                    RemoteAsyncDelegateMessage RemoteDel = new RemoteAsyncDelegateMessage(obj.ReceiveMessage);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(msg, RemoteCallback, null);
                }
                catch (SocketException)
                {
                    Console.WriteLine("-->The Server with the address {0} does not respond.", item.IP);

                }
            }
            Console.WriteLine("#End Sending Messages");

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

        #endregion

        #region INBOUND - IServerServer Members

        public void ReceiveFriendRequest(Contact c)
        {
            Console.WriteLine("<--Recebi pedido de amizade de: " + c.Username + " com o endereço: " + c.IP);
            lock (Server.State.FriendRequests)
            {
                Server.State.FriendRequests.Add(c);
            }
            var lc = new List<Contact>();
            lc.Add(c);

            //verifica se esta um cliente ligado, para actualizar a sua interface
            if (ServerClient.Client != null)
                ServerClient.Client.UpdateFriendRequest(lc);
        }
        public void ReceiveFriendRequestOK(Contact c)
        {
            Console.WriteLine("<--Recebi confirmação do pedido de amizade de: " + c.Username + " com o endereço: " + c.IP);

            ThreadPool.QueueUserWorkItem((object o) =>
            {
                var s = "YUPI " + c.Username + "(" + c.IP.Trim() + ")" + " is now friend of " + Server.State.Profile.UserName + ".";
                var msg = Server.State.MakeMessage(s);
                Server.State.Messages.Add(msg);

                var lm = new List<Message>();
                lm.Add(msg);
                ServerClient.Client.UpdatePosts(lm);
                ServerClient.Client.UpdateFriends(c);

                SendMessage(msg);

                lock (Server.State.Contacts)
                {
                    Server.State.Contacts.Add(c);
                }
            });

        }

        //FALTA: verificar msgs antigas que possa ter chegado
        public void ReceiveMessage(Message msg)
        {
            Console.WriteLine("<--Received post from:{0} Post:{1}", msg.FromUserName, msg.Post);
            lock (Server.State.Messages)
            {
                Server.State.Messages.Add(msg);
            }
            var lm = new List<Message>();
            lm.Add(msg);
            ServerClient.Client.UpdatePosts(lm);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion
    }
}
