using System;
using System.Collections.Generic;
using CommonTypes;
using System.Net.Sockets;
using System.Linq;
using System.Threading;

namespace Server
{
    public partial class ServerServer : MarshalByRefObject, IServerServer
    {
        public ServerClient ServerClient;
        public ChordNode node;

        //diz respeito à procura
        Dictionary<string, List<string>> iddMSexo;

        public ServerServer(ServerClient sc)
        {
            ServerClient = sc;
            node = new ChordNode();
            iddMSexo=new Dictionary<string,List<string>>();
        }

        /// <summary>
        /// OUTBOUND
        /// </summary>

        public void SendFriendRequest(string IP)
        {
            Console.WriteLine("-->Sending Friend Request.");
            var obj = (IServerServer)Activator.GetObject(
               typeof(IServerServer),
               string.Format("tcp://{0}/IServerServer", IP));
            try
            {
                AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackContact);
                RemoteAsyncDelegateContact RemoteDel = new RemoteAsyncDelegateContact(obj.ReceiveFriendRequest);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(Server.State.MakeContact(), RemoteCallback, null);
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
            var res = new List<Message>();
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

        public void BroadCastMessage(Message msg)
        {
            Console.WriteLine("#Start BroadCasting Message with SeqNumber:" + msg.SeqNumber);
            foreach (var item in Server.State.Contacts)
            {
                //TODO: IP do Servidor do Cliente ou IP do Clente??
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

        /// <summary>
        /// INBOUND
        /// </summary>

        public void ReceiveFriendRequest(Contact c)
        {
            Console.WriteLine("<--Received FR from: " + c.Username + ",address: " + c.IP + ", SeqNumber:" + c.LastMsgSeqNumber);

            Server.State.AddFriendInvitation(c);
            var lc = new List<Contact>();
            lc.Add(c);

            //Pedreiro -verifica se esta um cliente ligado, para actualizar a sua interface
            if (ServerClient.Client != null)
                ServerClient.Client.UpdateFriendInvitation(lc);
        }

        public void ReceiveFriendRequestOK(Contact c)
        {
            Console.WriteLine("<--Received confirmation of a FR send to:" + c.Username + ",address:" + c.IP + ",SeqNumber:" + c.LastMsgSeqNumber);
            ThreadPool.QueueUserWorkItem((object o) =>
            {
                var s = "YUPI!! I have a new friend: " + c.Username + "(" + c.IP.Trim() + ").";
                var msg = Server.State.MakeMessage(s);

                Server.State.AddMessage(msg);
                Server.State.RemoveFriendRequest(c);
                var lm = new List<Message>();
                lm.Add(msg);
                ServerClient.Client.UpdatePosts(lm);
                ServerClient.Client.UpdateFriends(c);
                BroadCastMessage(msg);
                Server.State.AddContact(c);
            });
        }

        public void ReceiveMessage(Message msg)
        {
            Console.WriteLine("<--Received post from:{0} SeqNumber:{1} Post:{2}", msg.FromUserName, msg.SeqNumber, msg.Post);
                Contact c = Server.State.Contacts.First(x => x.Username.Equals(msg.FromUserName));
                if (msg.SeqNumber == c.LastMsgSeqNumber + 1)
                {
                    Server.State.AddMessage(msg);
                    Server.State.UpdateSeqNumber(c, msg.SeqNumber);
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
                    var aux = SendRequestMessages(c.IP, c.LastMsgSeqNumber);
                    RefreshLocalMessages(aux, c);
                }
        }

        public IList<Message> RequestMessages(int lastSeqNumber)
        {
            var res = Server.State.Messages.Where(x => x.FromUserName.Equals(Server.State.Profile.UserName) && x.SeqNumber > lastSeqNumber).ToList();
            Console.WriteLine("Server: Sending missing Messages.");
            return res;
        }

        public override object InitializeLifetimeService() { return null; }

        public void RefreshLocalMessages(IList<Message> msgs, Contact c)
        {
            if (msgs != null && msgs.Count > 0)
            {
                foreach (var item in msgs)
                {
                    Server.State.AddMessage(item);
                }
                Server.State.UpdateSeqNumber(c, msgs.Max(x => x.SeqNumber));

                //pedreiro
                if (ServerClient.Client != null)
                    ServerClient.Client.UpdatePosts(msgs);
            }
        }

    }
}
