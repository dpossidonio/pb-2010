using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Net.Sockets;
using System.Collections;

namespace Client
{
    public class Client : MarshalByRefObject, IClient
    {
        public IServerClient Server { get; set; }
        public string ClientAddress { get; set; }
        public string ConnectedToServer { get; set; }
        public Queue<string> ServerAdress { get; set; }
        public UIClient ClientForm;
        private bool InitChannel;

        public Profile Profile { get; set; }
        public IList<Contact> Friends { get; set; }
        public IList<Contact> FriendsRequests { get; set; }
        public IList<Message> Messages { get; set; }

        public Client(UIClient form)
        {
            ClientForm = form;
            ServerAdress = new Queue<string>();
            InitChannel = false;
        }

        public void Connect(string ip)
        {
            ClientAddress = ip;
            if (!InitChannel)
                RegChannel();

            ConnectToServer();

            Profile = Server.GetProfile();
            ClientForm.LoadProfile(Profile);

            Messages = Server.GetMessages();
            ClientForm.UpdateMessageBox();
            
            Friends = Server.GetFriendsContacts();
            ClientForm.UpdateFriendsContacts(Friends);

            FriendsRequests = Server.GetPendingInvitations();
            ClientForm.UpdateFriendsRequests(FriendsRequests);
            
        }

        public void GetServersAdress(string srv1, string srv2, string srv3)
        {
            ServerAdress.Clear();
            if (!srv1.Equals(""))
                ServerAdress.Enqueue(srv1);
            if (!srv2.Equals(""))
                ServerAdress.Enqueue(srv2);
            if (!srv3.Equals(""))
                ServerAdress.Enqueue(srv3);
        }

        public void ConnectToServer()
        {
            while (ServerAdress.Count != 0)
            {
                ConnectedToServer = ServerAdress.Dequeue();
                try
                {
                    Server = (IServerClient)Activator.GetObject(
                     typeof(IServerClient),
                     string.Format("tcp://{0}/IServerClient", ConnectedToServer));
                    Server.Connect(this.ClientAddress);
                    ClientForm.UpdateServerInformation();
                    return;
                }
                catch (SocketException)
                {
                    //Servidor nao disponivel, tenta outro
                }                           
            }
                throw new NoServersAvailableException();
        }

        private void RegChannel()
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            IDictionary props = new Hashtable();
            var adr = this.ClientAddress;
            props["port"] = int.Parse(adr.Split(':')[1]);
            props["timeout"] = 5000; // in miliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, false);
            InitChannel = true;

            RemotingServices.Marshal(this, "IClient", typeof(IClient));
        }


        #region IClient Members

        void IClient.UpdateFriendInvitation(IList<Contact> FriendRequests)
        {
            ClientForm.UpdateFriendsRequests(FriendRequests);
        }

        void IClient.UpdatePosts(IList<Message> NewPosts)
        {
            foreach (var item in NewPosts)
            {
                Messages.Add(item);
            }
            ClientForm.UpdateMessageBox();
        }

        void IClient.UpdateFriends(Contact friend)
        {
            ClientForm.UpdateFriendsContacts(friend);
            this.Friends.Add(friend);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }



        public void ServiceAvailable(IList<string> servers)
        {
            foreach (var item in servers)
            {
                ServerAdress.Enqueue(item);
            }
            ClientForm.ServiceAvailableShow();
        }

        #endregion
    }
}
