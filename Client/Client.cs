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
    public class Client : MarshalByRefObject
    {
        public IServer Server { get; set; }
        public Queue<string> ServerAdress { get; set; }
        public UIClient ClientForm;

        public Profile Profile { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<Profile> Contacts { get; set; }

        public Client(UIClient form)
        {
            ClientForm = form;
            ServerAdress = new Queue<string>();
        }

        public void Connect(string ip)
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            IDictionary props = new Hashtable();
            props["port"] = int.Parse(ip.Split(':')[1]);
            props["timeout"] = 1000; // in miliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            //RemotingConfiguration.RegisterWellKnownServiceType(typeof(Client),
            //                                    "Client",
            //                                    WellKnownObjectMode.Singleton);
            //TODO
            ConnectToServer();

            try
            {
                Messages = Server.Connect(ip);
                Profile = Server.GetProfile();
                ClientForm.UpdateMessageBox(Messages);
                ClientForm.LoadProfile(Profile);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Could not locate server");
            }
        }

        public void GetServersAdress(string srv1, string srv2, string srv3)
        {
            if (srv1 != null)
                ServerAdress.Enqueue(srv1.ToString());
            if (srv2 != null)
                ServerAdress.Enqueue(srv2);
            if (srv3 != null)
                ServerAdress.Enqueue(srv3);
        }

        private void ConnectToServer()
        {
            Server = (IServer)Activator.GetObject(
                 typeof(IServer),
                    string.Format("tcp://{0}/Server", ServerAdress.Dequeue()));
        }
    }
}
