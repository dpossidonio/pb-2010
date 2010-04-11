using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonTypes;

namespace Client
{
    public partial class UIClient : Form
    {
        private string _client = "PADIbook - Client";
        private bool Connected { get; set; }
        private Client Client;

        public UIClient()
        {
            InitializeComponent();
            Init();
            Client = new Client(this);
        }

        private void Init()
        {
            Connected = false;
            GenderComboBox.DataSource = Enum.GetNames(typeof(CommonTypes.Gender));
            GenderComboBox.SelectedIndex = -1;
            InterestsComboBox.DataSource = Enum.GetNames(typeof(CommonTypes.Interest));
            InterestsComboBox.SelectedIndex = -1;
            for (int i = 1; i < 101; i++)
            {
                AgeComboBox.Items.Add(i);
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (Server1IPtextBox.Text.Equals(""))
                MessageBox.Show("Server Address \"#1\" should not empty!");
            else
            {
                Client.GetServersAdress(Server1IPtextBox.Text, Server2IPtextBox.Text, Server3IPtextBox.Text);
                Client.Connect(IPtextBox.Text);
                ConnectButton.Visible = false;
                this.Text = _client + " - Connected";
            }
        }

        public void UpdateMessageBox(IList<CommonTypes.Message> messages)
        {
            this.Invoke(new Action(delegate()
            {
                foreach (var item in messages)
                {
                    WallTextBox.Text += "\r\n" + DateTime.Now.ToShortTimeString() + " From:" + item.FromUserName + " - " + item.Post;
                }
            }));
        }

        public void LoadProfile(Profile p)
        {
            this.Invoke(new Action(delegate()
            {
                UserNameTextBox.Text = p.UserName;
                AgeComboBox.SelectedIndex = p.Age - 1;
                GenderComboBox.SelectedIndex = p.Gender.GetHashCode();
                UpdateInterests();
                InterestsComboBox.DataSource = Enum.GetNames(typeof(CommonTypes.Interest));
            }));
        }




        private void UpdateInterests()
        {
            InterestsTextBox.Text = "";
            foreach (var item in Client.Profile.Interests)
            {
                InterestsTextBox.Text += item.ToString() + ",";
            }
        }

        private void AddInterestsButton_Click(object sender, EventArgs e)
        {
            var a = Enum.Parse(typeof(CommonTypes.Interest), InterestsComboBox.SelectedItem.ToString());
            Client.Profile.Interests.Add((Interest)a);
            UpdateInterests();
        }

        private void UpdateProfileButton_Click(object sender, EventArgs e)
        {
            Client.Server.UpdateProfile(Client.Profile);
        }

        private void SendMessageButton_Click(object sender, EventArgs e)
        {
            Client.Server.Post(MessageTextBox.Text);
        }
    }
}
