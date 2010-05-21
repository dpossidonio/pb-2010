using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CommonTypes;
using System.Net.Sockets;
using System.Runtime.Remoting;

namespace Client
{
    public partial class UIClient : Form
    {
        private string _client = "PADIbook - Client";
        private string Status;
        private bool Connected { get; set; }
        private Client Client;
        public List<Interest> InterestsToUpdate = new List<Interest>();


        public UIClient(string address, IList<string> server_address)
        {
            InitializeComponent();
            Init(address, server_address);
            Client = new Client(this);
            Status = "OK";
        }

        private void Init(string address, IList<string> server_address)
        {
            if (!address.Equals(""))
            {
                IPtextBox.Text = address;
                switch (server_address.Count)
                {
                    case 1: 
                        Server1IPtextBox.Text = server_address[0];
                        Server2IPtextBox.Text = "";
                        Server3IPtextBox.Text = "";
                        break;
                    case 2: 
                        Server1IPtextBox.Text = server_address[0];
                        Server2IPtextBox.Text = server_address[1];
                        Server3IPtextBox.Text = "";
                        break;
                    case 3: 
                        Server1IPtextBox.Text = server_address[0];
                        Server2IPtextBox.Text = server_address[1];
                        Server3IPtextBox.Text = server_address[2];
                        break;
                    default: break;

                }
            }
            Connected = false;
            GenderComboBox.DataSource = Enum.GetNames(typeof(CommonTypes.Gender));
            GenderComboBox.SelectedIndex = -1;
            var interests = Enum.GetNames(typeof(CommonTypes.Interest)).ToList();

            //call
            FillInterestsComboBox(interests);
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
                    
                }
                catch (ServiceNotAvailableException)
                {
                    ServiceNotAvailableShow();
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not locate server.");
                }
            }
        }

        public void UpdateServerInformation() {
            this.Invoke(new Action(delegate()
            {
                this.Text = _client + " "+ Client.ClientAddress+" -> Server :" + Client.ConnectedToServer+" Status:"+Status;
            }));
        }

        public void UpdateMessageBox()
        {
            this.Invoke(new Action(delegate()
            {
                richWallTextBox.Clear();
                
                foreach (var m in Client.Messages.OrderBy(x => x.Time))
                {
                    richWallTextBox.Text += m.Time.ToShortTimeString() + " - From: " + m.FromUserName + " - " + m.Post + "\r\n";
                }
                richWallTextBox.SelectionStart = richWallTextBox.Text.Length;
                richWallTextBox.ScrollToCaret();
            }));
        }

        private void SendMessageButton_Click(object sender, EventArgs e)
        {
            if (Connected && !MessageTextBox.Text.Equals(""))
            {
                try
                {
                    //ao mandar uma msg ele retorna a msg k mandou que é adicionada à lista de msg local e publicada na wall
                    var m = Client.Server.Post(MessageTextBox.Text);
                    Client.Profile.PostSeqNumber = m.SeqNumber;
                    MessageTextBox.Text = "";
                    Client.Messages.Add(m);
                    UpdateMessageBox();               
               }
                catch (ServiceNotAvailableException)
                {
                    ServiceNotAvailableShow();
                }
                //EM MODO DEBUG com breakpoint's no Servidor Não!
                catch (SocketException)
                {
                    if(ReConnectClient())
                    this.SendMessageButton_Click(sender, e);
                }
            }
        }

        private void listContacts_Click(object sender, EventArgs e)
        {
            string contacts = "My Contacts:\n\n";
            foreach (var item in Client.Friends)
            {
                contacts += "\r\n" + item.ToString();
            }
            if (Client.Friends.Count == 0)
                contacts += "SNIFF... I dont have any friends.";
            try
            {
                var m = Client.Server.Post(contacts);
                Client.Messages.Add(m);
                MessageTextBox.Text = "";
                UpdateMessageBox();
            }
            catch (ServiceNotAvailableException)
            {
                ServiceNotAvailableShow();
            }
            catch (Exception) {
                if(ReConnectClient())
                listContacts_Click(sender, e);
            }
        }

        private void RefreshViewButton_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                try
                {
                    Client.Server.RefreshView();
                }
                catch (ServiceNotAvailableException)
                {
                    ServiceNotAvailableShow();
                }
                catch (Exception) {
                    if(ReConnectClient())
                    RefreshViewButton_Click(sender, e);
                }
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

                UpdateInterests(Client.Profile.Interests);
            }));
        }

        private void FillInterestsComboBox(List<string> interests) {
            InterestsComboBox.Items.Clear();
            foreach (var interest in interests)
            {
                InterestsComboBox.Items.Add(interest);
            }
        }


        private void UpdateInterests(List<Interest> list)
        {
            var all_interests = Enum.GetNames(typeof(CommonTypes.Interest)).ToList();
            var profile_interests = new List<string>();
            InterestsTextBox.Text = "";
   
            foreach (var item in list)
            {
                profile_interests.Add(item.ToString());        
                InterestsTextBox.Text += item.ToString() + ",";
            }
            List<string> equal = all_interests.Except(profile_interests).ToList();
            FillInterestsComboBox(equal);
        }

        private void AddInterestsButton_Click(object sender, EventArgs e)
        {
            if (Connected && InterestsComboBox.SelectedItem != null)
            {
                //if (Client.Profile.Interests.Count < 6)
                //{
                //    var a = Enum.Parse(typeof(CommonTypes.Interest), InterestsComboBox.SelectedItem.ToString());
                //    Client.Profile.Interests.Add((Interest)a);
                //    InterestsComboBox.Items.Remove(InterestsComboBox.SelectedItem);
                //    UpdateInterests();
                //}
                if ((InterestsToUpdate.Count + Client.Profile.Interests.Count) < 6)
                {
                    Interest a = (Interest)Enum.Parse(typeof(CommonTypes.Interest), InterestsComboBox.SelectedItem.ToString());
                    InterestsToUpdate.Add(a);
                    InterestsComboBox.Items.Remove(InterestsComboBox.SelectedItem);
                    UpdateInterests(Client.Profile.Interests.Union(InterestsToUpdate).ToList());
                }
                else MessageBox.Show("It's only possible to add 6 Interests!");
            }
        }

        private void UpdateProfileButton_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                var new_profile = new Profile();
                //Eliminar se o nome for único e inalteravel
                new_profile.UserName = UserNameTextBox.Text;
                //
                new_profile.Age = AgeComboBox.SelectedIndex + 1;
                new_profile.Gender = (Gender)Enum.Parse(typeof(Gender), (string)GenderComboBox.SelectedValue);
                new_profile.IP = Client.Profile.IP;
                new_profile.Interests = Client.Profile.Interests.Union(InterestsToUpdate).ToList();
                new_profile.PostSeqNumber = Client.Profile.PostSeqNumber;
                try
                {
                    Client.Server.UpdateProfile(new_profile);
                    Client.Profile = new_profile;
                    InterestsToUpdate.Clear();
                }
                catch (ServiceNotAvailableException)
                {
                    ServiceNotAvailableShow();
                    InterestsToUpdate.Clear();
                    LoadProfile(Client.Profile);
                }
                catch (Exception) {
                    if(ReConnectClient())
                    UpdateProfileButton_Click(sender, e);
                }
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
                if (serverTextBox.Text.Equals("") || Client.Profile.UserName.Equals("") || serverTextBox.Text.Length != 14)
                    MessageBox.Show("Fill out all the fields!!");
                else
                {
                    try
                    {
                        Client.Server.PostFriendRequest(serverTextBox.Text);
                        serverTextBox.Text = "127.0.0.1:";
                    }
                    catch (ServiceNotAvailableException)
                    {
                        ServiceNotAvailableShow();
                    }
                    catch (Exception) {
                        if(ReConnectClient())
                        SendFriendReqButton_Click(sender, e);
                    }
                }
            }
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (friendsReqComboBox.SelectedItem != null)
            {
                var c = (Contact)friendsReqComboBox.SelectedItem;
                try
                {
                    var m = Client.Server.RespondToFriendRequest(c, true);
                    Client.Messages.Add(m);
                    UpdateFriendsContacts(c);
                    UpdateMessageBox();
                    friendsReqComboBox.Items.Remove(friendsReqComboBox.SelectedItem);
                    friendsReqComboBox.Text = "";
                }
                catch (ServiceNotAvailableException)
                {
                    ServiceNotAvailableShow();
                }
                catch (Exception) {
                    if(ReConnectClient())
                     AcceptButton_Click(sender,  e);
                }
            }
        }

        private void declineButton_Click(object sender, EventArgs e)
        {
            if (friendsReqComboBox.SelectedItem != null)
            {
                var c = (Contact)friendsReqComboBox.SelectedItem;
                try
                {
                    Client.Server.RespondToFriendRequest(c, false);
                    friendsReqComboBox.Items.Remove(friendsReqComboBox.SelectedItem);
                    friendsReqComboBox.Text = "";
                }
                catch (ServiceNotAvailableException)
                {
                    ServiceNotAvailableShow();
                }
                catch (Exception) {
                    if(ReConnectClient())
                    declineButton_Click(sender,e);
                }
                
            }
        }

        public void ServiceNotAvailableShow() {
            Status = "SERVICE NOT AVAILABLE";
            MessageBox.Show("Service Not Available!");
            UpdateServerInformation();
        }

        public void ServiceAvailableShow()
        {
            this.Invoke(new Action(delegate()
            {
            Status = "OK";
            UpdateServerInformation();
            }));
        }

        public bool ReConnectClient() {
            try
            {
                Client.ConnectToServer();
                return true;
            }
            catch (NoServersAvailableException) {

                Status = "Disconnected";
                Client.ConnectedToServer = "";
                MessageBox.Show("Could not locate Server");
                UpdateServerInformation();
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Search Content
        /// </summary>
        public void UpdateSearchResults(List<string> list, string type)
        {
            this.Invoke(new Action(delegate()
            {
                txtResults.Clear();
                if (type.Equals("name"))
                {
                    foreach (var v in list)
                    {
                        var aux = v.Split('@');
                        txtResults.Text += string.Format("Username: {0}\t Address: {1}\n", aux[0], aux[1]);
                    }
                    return;
                }
                if (type.Equals("sexage"))
                {
                    txtResults.Text = string.Format("\t\tSexAge:{0}\r\n\r\n", txtSexAgeSearch.Text);
                    if (list.Count == 0) txtResults.Text += "No users found!";
                    foreach (var v in list)
                    {
                        txtResults.Text += string.Format("Address: {0}\r\n", v);
                    }
                    return;
                }
                if (type.Equals("interests"))
                {
                    txtResults.Text = string.Format("\t\tInterests:{0}\r\n\r\n", txtInterestsSearch.Text);
                    if (list.Count == 0) txtResults.Text += "No users found!";
                    foreach (var v in list)
                    {
                        txtResults.Text += string.Format("Address: {0}\r\n", v);
                    }
                    return;
                }
            }));

        }

        private void bttSearchUsername_Click(object sender, EventArgs e)
        {
            Client.SearchByName(txtUsernameSearch.Text);
        }

        private void bttSearchSexAge_Click(object sender, EventArgs e)
        {
            Client.SearchBySexAge(txtSexAgeSearch.Text);
        }

        private void bttSearchInterest_Click(object sender, EventArgs e)
        {
            Client.SearchByInterest(txtInterestsSearch.Text);
        }

        

    }
}
