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
        private Server ServerClient;
        public ServerServer ServerServer;
        private bool _activated;
        private string _server = "PADIbook - Server";

        public UIServer()
        {
            InitializeComponent();
            ServerClient = new Server(this);
            ServerServer = new ServerServer(this);
            _activated = false;
        }

        private void ActivateServer()
        {
            TcpChannel channel = new TcpChannel(int.Parse(IPTextBox.Text.Split(':')[1]));
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this.ServerClient,
                "IServerClient",
                typeof(IServerClient));
           

            RemotingServices.Marshal(this.ServerServer,
                 "IServerServer",
              typeof(IServerServer));
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (_activated)
            {
                RemotingServices.Disconnect(this.ServerClient);
                this.Text = _server + " - OFF";
                ConnectButton.Text = "TurnON";
                _activated = false;
            }
            else{
                ActivateServer();
                _activated = true;
                this.Text = _server + " - ON";
                ConnectButton.Text = "TurnOFF";
            }

        }

        public void UpdateLog(string str)
        {
            this.Invoke(new Action(delegate()
            {
                LogTextBox.Text += "\n\t"+str;
            }));
        }
    }
}