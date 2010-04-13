using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        public static ServerState State;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome - PADIbook Server");

            //forma de obter o endereço ip da máquina sem ser hardcoded :P
            //System.Net.IPAddress[] a = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            //Console.WriteLine(a[2].ToString());

            var localIP = "127.0.0.1:";
            Console.Write("Enter Port to run: ");
            var porto = Console.ReadLine();
            var ip = string.Format(localIP + porto);
            Console.WriteLine("Running Server on: "+ip);
            State = new ServerState();
            var sc = new ServerClient(ip);

            while (true) {
                Console.ReadLine();
            }
        }
    }

    #region ServerState

    public class ServerState
    {
        public Profile Profile { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<Contact> Contacts { get; set; }
        public IList<Contact> FriendRequests { get; set; }

        public ServerState()
        {
            Profile = new Profile();
            Messages = new List<Message>();
            Contacts = new List<Contact>();
            FriendRequests = new List<Contact>();
        }

        public Message makeMessage(string msg)
        {
            var m = new Message();
            m.FromUserName = this.Profile.UserName;
            m.Post = msg;
            m.Time = DateTime.Now;
            return m;
        }

        //TODO - serializar a class
    }

    #endregion

    #region ServerClient

    public class ServerClient : MarshalByRefObject, IServerClient
    {
        public ServerServer ServerServer;

        public ServerClient(string ip)
        {
            ServerServer = new ServerServer();
            Registration(ip);
            test(ip);
        }

        private void Registration(string ip)
        {
            TcpChannel channel = new TcpChannel(int.Parse(ip.Split(':')[1]));
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(this,"IServerClient",typeof(IServerClient));
            RemotingServices.Marshal(this.ServerServer, "IServerServer", typeof(IServerServer));
        }

        //Classe única e exclusivamente para testes
        public void test(string IP)
        {
            //Test
            var p = new Profile();

            p.UserName = "David";
            p.Age = 25;
            p.IP = IP;
            p.Interests.Add(Interest.Movies);
            p.Interests.Add(Interest.Science);
            p.Gender = Gender.Male;

            var c = new Contact();
            c.IP = "127.0.0.1:1001";
            c.Username = "Pouca vergonha :P";

            var msg = new Message();
            msg.FromUserName = "System";
            msg.Post = "This is not a drill!! I repeat!! This is not a drill!!";
            msg.Time = DateTime.Now;

            //Server.State.Contacts.Add(c);
            Server.State.Profile = p;
            Server.State.Messages.Add(msg);
            //Server.State.FriendRequests.Add(c);

            //TextWriter tw = new StreamWriter("PADIdatabase.xml");
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(p.GetType());
            //x.Serialize(tw, p);
            //Console.WriteLine("object written to file");
            //Console.ReadLine();
            //tw.Close();
            //end Test
        }

        #region IServerClient Members

        public void Freeze() {}

        public Message Post(string message)
        {
            var m = Server.State.makeMessage(message);
            ServerServer.Send(m);
            return m;
        }

        public void PostFriendRequest(string username, string address) {
        //o username é desnecessário, só com o endereço do server dá para fazer o pedido

            var c = new Contact();
            c.Username = Server.State.Profile.UserName;
            c.IP = Server.State.Profile.IP;
            ServerServer.SendFriendRequest(c,address);
        }

        public Message RespondToFriendRequest(Contact c, bool accept) 
        {
            Message msg = new Message();
            //adicionar amigo
            if (accept)
            {
                var s = "YUPI " + c.Username + "(" + c.IP.Trim() + ")" + " is now friend of " + Server.State.Profile.UserName + ".";

                //ATENÇÃO É NECESSÁRIO VER SE ESTA É A MELHOR ORDEM DE FAZER ISTO
                //difundir nova amizade por todos os amigos
                msg = Server.State.makeMessage(s);
                Server.State.Messages.Add(msg);
                ServerServer.Send(Server.State.makeMessage(s));

                //adicionar novo amigo aos contactos e informa-lo da aceitação 
                Server.State.Contacts.Add(c);
                var myContact= new Contact();
                myContact.IP = Server.State.Profile.IP;
                myContact.Username = Server.State.Profile.UserName;
                ServerServer.SendFriendRequestConfirmation(myContact, c.IP.Trim());
                                
            }

            Server.State.FriendRequests.Remove(c);
            return msg;
        }

        public IList<Message> RefreshView() { return null; }

        public void UpdateProfile(Profile profile)
        {
            Server.State.Profile = profile;
        }

        public Profile GetProfile()
        {
            return Server.State.Profile;
        }

        public IList<Contact> GetFriendsContacts()
        {
            return Server.State.Contacts;
        }

        public IList<Contact> GetFriendsRequestsContacts()
        {
            return Server.State.FriendRequests;
        }

        public IList<Message> GetMessages()
        {
            return Server.State.Messages;
        }

        #endregion
    }

    #endregion

    #region ServerServer

    public class ServerServer : MarshalByRefObject, IServerServer
    {
        #region OUTBOUND

        #region Delegates & Asyncs

        private delegate void RemoteAsyncDelegateMessage(Message msg);
        private static void OurRemoteAsyncCallBackMessage(IAsyncResult ar)
        {
            RemoteAsyncDelegateMessage del = (RemoteAsyncDelegateMessage)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("Recebi confirmação de mensagem de um servidor");
            return;
        }

        //pode-se melhorar isto se der para passar qualquer coisa void* para o RemoteAsyncDelegate
        private delegate void RemoteAsyncDelegateContact(Contact c);
        private static void OurRemoteAsyncCallBackContact(IAsyncResult ar)
        {
            RemoteAsyncDelegateContact del = (RemoteAsyncDelegateContact)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("Recebi confirmação de um pedido de amizade de um servidor");
            return;
        }

        #endregion

        public void Send(Message msg)
        {
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
                catch (SocketException) { }
            }
            
        }

        public void SendFriendRequest(Contact c, string IP)
        {
            //verificar se é um ip correcto
            //System.Net.IPAddress ipAddress = null;
            //bool isValidIp = System.Net.IPAddress.TryParse(IP, out ipAddress);
            //if (isValidIp)
            {
                var obj = (IServerServer)Activator.GetObject(
                   typeof(IServerServer),
                   string.Format("tcp://{0}/IServerServer", IP));

                try
                {
                    AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackContact);
                    RemoteAsyncDelegateContact RemoteDel = new RemoteAsyncDelegateContact(obj.ReceiveFriendRequest);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(c, RemoteCallback, null);
                }
                catch (SocketException) { }
            }
        }

        public void SendFriendRequestConfirmation(Contact c, string IP)
        {
            var obj = (IServerServer)Activator.GetObject(
                   typeof(IServerServer),
                   string.Format("tcp://{0}/IServerServer", IP));

            try
            {
                AsyncCallback RemoteCallback = new AsyncCallback(ServerServer.OurRemoteAsyncCallBackContact);
                RemoteAsyncDelegateContact RemoteDel = new RemoteAsyncDelegateContact(obj.ReceiveFriendRequestOK);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(c, RemoteCallback, null);
            }
            catch (SocketException) { }
        
        }

        #endregion

        #region INBOUND - IServerServer Members

        public void ReceiveFriendRequest(Contact c)
        {
            Console.WriteLine("Recebi pedido de amizade de: " + c.Username + " com o endereço: " + c.IP);
            lock (Server.State.FriendRequests)
            {
                Server.State.FriendRequests.Add(c);
            }
        }

        public void ReceiveFriendRequestOK(Contact c)
        {
            Console.WriteLine("Recebi confirmação do pedido de amizade de: " + c.Username + " com o endereço: " + c.IP);

            var s = "YUPI " + c.Username + "(" + c.IP.Trim() + ")" + " is now friend of " + Server.State.Profile.UserName + ".";
            var msg = Server.State.makeMessage(s);
            Server.State.Messages.Add(msg);
            Send(msg);

            lock (Server.State.Contacts) { Server.State.Contacts.Add(c); }

        }

        public void ReceiveMessage(Message msg)
        {
            Console.WriteLine("Recebi: " + msg.Post);
            lock (Server.State.Messages)
            {
                Server.State.Messages.Add(msg);
            }
        }

        #endregion
    }

    #endregion

}
