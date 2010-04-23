using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonTypes;
using System.Net.Sockets;

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
            //Friends tab
            friendsTextBox.Text = "   Friend Server      -         Friend Username\r\n";
        }

        #region Home

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (Server1IPtextBox.Text.Equals(""))
                MessageBox.Show("Server Address \"#1\" should not empty!");
            else
            {
                Client.GetServersAdress(Server1IPtextBox.Text, Server2IPtextBox.Text, Server3IPtextBox.Text);
                try   
                {
                    Client.Connect(IPtextBox.Text);
                    ConnectButton.Visible = false;
                    Connected = true;
                    this.Text = _client + " - Connected - Server :"+ Client.ConnectedToServer;
                }
                catch (SocketException)
                {
                    MessageBox.Show("Could not locate server.");
                }
            }
        }

        public void UpdateMessageBox()
        {
            this.Invoke(new Action(delegate()
            {
            WallTextBox.Clear();
                //Client.Friends.
            foreach (var m in Client.Messages.OrderBy(x => x.Time))
            {
                    WallTextBox.Text += m.Time.ToShortTimeString() + " - From: " + m.FromUserName + " - " + m.Post + "\r\n";
            }
           }));
        }

        private void SendMessageButton_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                //ao mandar uma msg ele retorna a msg k mandou que é adicionada à lista de msg local e publicada na wall
                var m = Client.Server.Post(MessageTextBox.Text);
                MessageTextBox.Text = "";
                Client.Messages.Add(m);
                UpdateMessageBox();
            }
        }

        private void listContacts_Click(object sender, EventArgs e)
        {
            string contacts = "My Contacts:\n\n";
            Client.Friends = Client.Server.GetFriendsContacts();
            foreach (var item in Client.Friends)
            {
                contacts = contacts + "\r\n" + item.ToString();
            }
            var m = Client.Server.Post(contacts);
            MessageTextBox.Text = "";
            Client.Messages.Add(m);
            UpdateMessageBox();
        }

        private void RefreshViewButton_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                Client.Server.RefreshView();   
            }
        }

        #endregion

        #region Profile

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
            if (Connected)
            {
                var a = Enum.Parse(typeof(CommonTypes.Interest), InterestsComboBox.SelectedItem.ToString());
                Client.Profile.Interests.Add((Interest)a);
                UpdateInterests();
            }
        }

        private void UpdateProfileButton_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                //Eliminar se o nome for único e inalteravel
                Client.Profile.UserName = UserNameTextBox.Text;
                //
                Client.Profile.Age = AgeComboBox.SelectedIndex + 1;
                Client.Profile.Gender = (Gender)Enum.Parse(typeof(Gender), (string)GenderComboBox.SelectedValue);
                Client.Server.UpdateProfile(Client.Profile);
            }
        }

        #endregion

        #region Friends

        public void UpdateFriendsContacts(Contact c)
        {
            this.Invoke(new Action(delegate()
           {
               friendsTextBox.Text += "\r\n " + c.IP + "     -     " + c.Username;
           }));

        }
        public void UpdateFriendsContacts(IList<Contact> c)
        {
            foreach (var item in c)
            {
                UpdateFriendsContacts(item);
            }
        }

        public void UpdateFriendsRequests(IList<Contact> c)
        {
            this.Invoke(new Action(delegate()
            {
                foreach (var item in c)
                {
                    friendsReqComboBox.Items.Add(item);
                }
            }));
        }

        private void SendFriendReqButton_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                if (serverTextBox.Text.Equals("") || Client.Profile.UserName.Equals(""))
                    MessageBox.Show("Fill out all the fields!!");
                else
                {
                    try
                    {
                        Client.Server.PostFriendRequest(serverTextBox.Text);
                        serverTextBox.Text = "127.0.0.1:";
                    }
                    catch (SocketException)
                    {
                        // E AGORA???
                    }
                }
            }
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (friendsReqComboBox.SelectedItem != null)
            {
                var c = (Contact)friendsReqComboBox.SelectedItem;
                var m = Client.Server.RespondToFriendRequest(c, true);
                Client.Friends.Add(c);
                Client.Messages.Add(m);
                UpdateFriendsContacts(c);
                UpdateMessageBox();
                friendsReqComboBox.Items.Remove(friendsReqComboBox.SelectedItem);
                friendsReqComboBox.Text = "";
            }
        }

        private void declineButton_Click(object sender, EventArgs e)
        {
            if (friendsReqComboBox.SelectedItem != null)
            {
                var c = (Contact)friendsReqComboBox.SelectedItem;
                friendsReqComboBox.Items.Remove(friendsReqComboBox.SelectedItem);
                friendsReqComboBox.Text = "";
                Client.Server.RespondToFriendRequest(c, false);
            }
        }

        #endregion



    }
}
