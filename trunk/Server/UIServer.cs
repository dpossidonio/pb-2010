using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using CommonTypes;

namespace Server
{
    public partial class UIServer : Form
    {
        private Server Server;
        private bool Activated;
        private string _server = "PADIbook - Server";

        public UIServer()
        {
            InitializeComponent();
            Server = new Server(this);
            Activated = false;
        }

        private void ActivateServer()
        {
            TcpChannel channel = new TcpChannel(int.Parse(IPTextBox.Text.Split(':')[1]));
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this.Server,
                "Server",
                typeof(Server));
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (Activated)
            {
                RemotingServices.Disconnect(this.Server);
                this.Text = _server + " - OFF";
                ConnectButton.Text = "TurnON";
                Activated = false;
            }
            else{
                ActivateServer();
                Activated = true;
                this.Text = _server + " - ON";
                ConnectButton.Text = "TurnOFF";
            }

        }
    }
}
