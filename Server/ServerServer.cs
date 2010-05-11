﻿using System;
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
        public ChordNode node;

        public ServerServer(ServerClient sc)
        {
            ServerClient = sc;
            node = new ChordNode();
        }

        /// <summary>
        /// OUTBOUND
        /// </summary>

#region OUTBOUND

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

        private delegate void RemoteAsyncDelegateUpdateContact(Contact c,bool b);
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

        #region Replication
        //REPLICAÇÂO
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
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(p, m, c,fr,pi, RemoteCallback, null);
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

        public void SetContact(List<string> replicas, CommonTypes.Contact c,bool updateSeqNumber)
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
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c,updateSeqNumber, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                }
            }
            Console.WriteLine("#End REP Contact");
        }

        public void SetFriendRequest(List<string> replicas, CommonTypes.Contact c,bool b)
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
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c,b, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                }
            }
            Console.WriteLine("#End REP Friend Request");
        }

        public void SetFriendInvitation(List<string> replicas, CommonTypes.Contact c,bool b)
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
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c,b, RemoteCallback, null);
                }
                catch (Exception)
                {
                    Console.WriteLine("-->The Replicated Server with the address {0} does not respond.", item);
                }
            }
            Console.WriteLine("#End REP FriendInvitation");
        }

        //FIM REPLICAÇÂO
        #endregion

        #region ChordNodeStuff

        public void ChordJoin(string ip)
        {
           node.SetID(Server.State.Profile.UserName);
           node.Predecessor = (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", ip));
           object[] o = node.Predecessor.ChordNodeRequestingToJoin(Server.State.ServerIP);
           node.Sucessor = (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", o[0]));
           node.Sucessor2 = (IServerServer)Activator.GetObject(typeof(IServerServer),string.Format("tcp://{0}/IServerServer", o[1]));
           
        }

        public void ChordLeave()
        {

        }

        //função de debug para ver se o anel está realmente bem formado
        public string PrintSucessores()
        {
            string s = "none";
            if (node.HasSucessor() && node.HasSucessor2())
            {
                s = "Sucessor: " + node.Sucessor.GetServerIP() +"\nSucessor2: " + node.Sucessor2.GetServerIP() +"\nPredecessor: " + node.Predecessor.GetServerIP();
                return s;
            }
            else if (node.HasSucessor())
            {
                s = "Sucessor: " + node.Sucessor.GetServerIP() + "\nPredecessor: " + node.Predecessor.GetServerIP();
                return s;
            }
            else
                return s;
        }

        #endregion

#endregion

        /// <summary>
        /// INBOUND
        /// </summary>

#region INBOUND - IServerServer Members

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

        #region REPLICAÇÂO
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

        public void UpdateContacts(Contact c,bool updateSeqNumber)
        {
            if (updateSeqNumber) Server.State.UpdateSeqNumber(c,c.LastMsgSeqNumber);
            else Server.State.AddContact(c);
        }

        public void UpdateProfile(Profile p)
        {
            Server.State.UpdateProfile(p);
        }

        public void UpdateFriendRequest(Contact c,bool b)
        {
            if (b) Server.State.AddFriendRequest(c);
            else Server.State.RemoveFriendRequest(c);
        }

        public void UpdatePendingInvitation(Contact c,bool b)
        {
            if (b) Server.State.AddFriendInvitation(c);
            else Server.State.RemoveFriendInvitation(c);
        }

        #endregion

        #region ChordNode

        //funções auxiliares
        public void SetSucessor(string ip) { node.Sucessor = (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", ip)); }
        public void SetSucessor2(string ip) { node.Sucessor2 = (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", ip)); }
        public void SetPredecessor(string ip) { node.Predecessor = (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", ip)); }
        public string GetServerIP() { return Server.State.ServerIP; }

        public object[] ChordNodeRequestingToJoin(string ip)
        {
            //tá implementado de forma a que o nó que pede para entrar é posto à frente do nó que já estava no anel

            var v = (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", ip));
            //lista de sucessores que tinha
            if (!node.HasPredecessor() && !node.HasSucessor2() && !node.HasSucessor())
            {
                node.Predecessor = v;
                node.Sucessor = v;
                node.Sucessor2 = (IServerServer)Activator.GetObject(typeof(IServerServer),
                        string.Format("tcp://{0}/IServerServer", Server.State.ServerIP));

                object[] aux = {Server.State.ServerIP,ip };
                return aux;
            }

            object[] auxx = { node.Sucessor.GetServerIP(), node.Sucessor2.GetServerIP() };
            
            //o predecessor do meu sucessor agora é o novo no que pus no anel
            if(node.HasSucessor())
                node.Sucessor.SetPredecessor(ip);

            //tenho de alterar o meu predecessor e dizer que o seu novo sucessor2 é quem eu pus no anel
            if(node.HasPredecessor())
                node.Predecessor.SetSucessor2(ip);

            //eu tenho de alterar os meus sucessores
            node.Sucessor2 = node.Sucessor;
            node.Sucessor = v;
       
            return auxx;
        }

        public void ChordNodeRequestingToLeave()
        {

        }



        #endregion

#endregion


    }
}
