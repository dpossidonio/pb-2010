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
                try   //ISTO DEVIA SER FEITO NA CLASSE CLIENT
                {
                    Client.Connect(IPtextBox.Text);
                    ConnectButton.Visible = false;
                    Connected = true;
                    this.Text = _client + " - Connected";
                }
                catch (SocketException) {
                    MessageBox.Show("Could not locate server.");
                }
            }
        }

        public void UpdateMessageBox(IList<CommonTypes.Message> m)
        {
            this.Invoke(new Action(delegate()
            {
                foreach (var item in m)
                {
                    WallTextBox.Text += item.Time + " - From: " + item.FromUserName + " - " + item.Post + "\r\n";
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
                var lm = new List<CommonTypes.Message>();
                lm.Add(m);
                UpdateMessageBox(lm);

                //WallTextBox.Text += "\r\n" + m.Time + " From: " + m.FromUserName + " - " + m.Post;
            }
        }

        private void RefreshViewButton_Click(object sender, EventArgs e)
        {
            if (Connected) { 
            //TODO
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
            var a = Enum.Parse(typeof(CommonTypes.Interest), InterestsComboBox.SelectedItem.ToString());
            Client.Profile.Interests.Add((Interest)a);
            UpdateInterests();
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

        public void UpdateFriendsContacts(IList<Contact> c)
        {
            this.Invoke(new Action(delegate()
            {
                foreach (var item in c)
                {
                    friendsTextBox.Text += "\r\n " + item.IP + "     -     " + item.Username;
                }    
            }));
        }

        public void UpdateFriendsRequests(IList<Contact> c)
        {
            this.Invoke(new Action(delegate()
            {
                foreach (var item in c)
                {
                    friendsReqComboBox.Items.Add(item.Username.ToString() + " , " + item.IP.ToString() );
                }
            }));
        }

        // FALTA TRATAR A EXcepção quando o FRiend nao esta disponivel e guardar o pedido
        // implementar isto na classe Client?
        private void SendFriendReqButton_Click(object sender, EventArgs e)
        {
            if (userTextBox.Text.Equals("") || serverTextBox.Text.Equals("") || Client.Profile.UserName.Equals(""))
                MessageBox.Show("Fill out all the fields!!");
            else
            {
                try
                {
                    Client.Server.PostFriendRequest(userTextBox.Text, serverTextBox.Text);
                    userTextBox.Text = "";
                    serverTextBox.Text = "";
                }catch(SocketException){
                // E AGORA???
                }
            }
        }

        private Contact MakeContact()
        {
            //new friend contact
            var friend = new Contact();
            //HA UMA MELHOR MANEIRA DE FAZER ISTO...
            string[] words = friendsReqComboBox.SelectedItem.ToString().Split(',');
            friend.Username = words[0];
            friend.IP = words[1];

            //remover entrada da caixa de pedidos local
            friendsReqComboBox.Items.Remove(friendsReqComboBox.SelectedItem);
            friendsReqComboBox.Text = "";

            return friend;
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (friendsReqComboBox.SelectedItem != null)
            {
                var friend = MakeContact();
                var m = Client.Server.RespondToFriendRequest(friend, true);

                //CODIGO REPETIDO??
                friendsTextBox.Text += "\r\n" + friend.IP + "     -     " + friend.Username;
                WallTextBox.Text += m.Time + " From: " + m.FromUserName + " - " + m.Post + "\r\n";
            }
        }

        private void declineButton_Click(object sender, EventArgs e)
        {
            if (friendsReqComboBox.SelectedItem != null)
            {
                var friend = MakeContact();
                Client.Server.RespondToFriendRequest(friend, false);
            }
        }

        #endregion        

        
    }
}
