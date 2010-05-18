using CommonTypes;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Threading;
using System.Linq;

namespace Server
{
    public class ServerState
    {
        private Profile _profile;
        private IList<Message> _messages;
        private IList<Contact> _contacts;
        //Pedidos enviados
        private IList<Contact> _friendRequests;
        //Pedidos recebidos
        private IList<Contact> _pendingInvitations;

        public string ServerIP { get; set; }
        private XmlSerializer Serializer;
        public List<string> ReplicationServers { get; set; }
        public int Delay { get; set; }
        public int FreezePeriod { get; set; }
        public DateTime FreezeTimeOver { get; set; }

        public void VerifyFreeze()
        {
            if (FreezeTimeOver >= DateTime.Now)
                Thread.Sleep(Server.State.Delay);
        }

        public ServerState(string ip)
        {
            ServerIP = ip;
            _profile = new Profile();
            _messages = new List<Message>();
            _contacts = new List<Contact>();
            _friendRequests = new List<Contact>();
            _pendingInvitations = new List<Contact>();
            ReplicationServers = new List<string>();
        }

        public Profile Profile
        {
            get { return _profile; }
            set
            {
                lock (Profile)
                {
                    _profile = value;
                    SerializeObject(Profile, "Profile");
                }
            }
        }

        public IList<Message> Messages
        {
            get { return _messages; }
            set
            {
                lock (Messages)
                {
                    _messages = value;
                    SerializeObject(Messages, "Messages");
                }
            }
        }

        public IList<Contact> Contacts
        {
            get { return _contacts; }
            set
            {
                lock (Contacts)
                {
                    _contacts = value;
                    SerializeObject(Contacts, "Contacts");
                }
            }
        }

        public IList<Contact> FriendRequests
        {
            get { return _friendRequests; }
            set
            {
                lock (FriendRequests)
                {
                    _friendRequests = value;
                    SerializeObject(FriendRequests, "FriendRequests");
                }

            }
        }

        public IList<Contact> PendingInvitations
        {
            get { return _pendingInvitations; }
            set
            {
                lock (PendingInvitations)
                {
                    _pendingInvitations = value;
                    SerializeObject(PendingInvitations, "PendingInvitations");
                }
            }
        }

        public Message MakeMessage(string msg)
        {
            var m = new Message();
            m.FromUserName = this.Profile.UserName;
            m.Post = msg;
            m.Time = DateTime.Now;
            m.SeqNumber = ++this.Profile.PostSeqNumber;
            return m;
        }

        public void AddMessage(Message m)
        {  //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterMessage(m));
        }

        public void CommitMessage(Message m)
        {
            lock (Messages)
            {
                Messages.Add(m);
                SerializeObject(Messages, "Messages");
            }
        }


        public Contact MakeContact()
        {
            var myContact = new Contact();
            //Conhece apenas o endereço do servidor do client
            myContact.IP = Server.State.ServerIP;
            myContact.Username = Server.State.Profile.UserName;
            //Enviar o numero de sequencia da ultima mensagem? ou 0 para o amigo pedir todas os post's
            myContact.LastMsgSeqNumber = Server.State.Profile.PostSeqNumber;
            return myContact;
        }

        public void AddContact(Contact c)
        {
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterContact(c));
        }

        public void CommitContact(Contact c) {
            lock (Contacts)
            {
                Contacts.Add(c);
                SerializeObject(Contacts, "Contacts");
            }    
        }

        public void UpdateProfile(Profile p)
        {
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterProfile(p));
        }

        public void CommitProfile(Profile p) {
            lock (Profile)
            {
                _profile = p;
                SerializeObject(Profile, "Profile");
            }
        }

        public void AddFriendRequest(Contact c)
        {
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterFriendRequest(c, true));
        }

        public void CommitAddFriendRequest(Contact c)
        {
            lock (FriendRequests)
            {
                FriendRequests.Add(c);
                SerializeObject(FriendRequests, "FriendRequests");
            }
        }

        public void RemoveFriendRequest(Contact c)
        {
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterFriendRequest(c, false));
        }


        public void CommitRemoveFriendRequest(Contact c)
        {
            var caux = FriendRequests.First(x => x.IP.Equals(c.IP));
            lock (FriendRequests)
            {
                FriendRequests.Remove(caux);
                SerializeObject(FriendRequests, "FriendRequests");
            }
        }


        public void AddFriendInvitation(Contact c)
        {
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterPendingInvitation(c, true));
        }

        public void CommitAddFriendInvitation(Contact c)
        {
            lock (PendingInvitations)
            {
                PendingInvitations.Add(c);
                SerializeObject(PendingInvitations, "PendingInvitations");
            }
        }

        public void RemoveFriendInvitation(Contact c)
        {
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterPendingInvitation(c, false));
        }

        public void CommitRemoveFriendInvitation(Contact c)
        {
            var caux = PendingInvitations.First(x => x.IP.Equals(c.IP));
            lock (PendingInvitations)
            {
                PendingInvitations.Remove(caux);
                SerializeObject(PendingInvitations, "PendingInvitations");
            }
        }

        public void UpdateSeqNumber(Contact c, int seqNumber)
        {
            var caux = Contacts.First(x => x.IP.Equals(c.IP));
            lock (Contacts)
            {
                caux.LastMsgSeqNumber = seqNumber;
                SerializeObject(Contacts, "Contacts");
            }
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterContact(c));
        }


        #region Serialize

        public void DeserializeState()
        {
            Server.State.Contacts = (IList<Contact>)Server.State.DeserializeObject(Server.State.Contacts, "Contacts");
            Server.State.Messages = (IList<Message>)Server.State.DeserializeObject(Server.State.Messages, "Messages");
            Server.State.UpdateProfile((Profile)Server.State.DeserializeObject(Server.State.Profile, "Profile"));
            Server.State.PendingInvitations = (IList<Contact>)Server.State.DeserializeObject(Server.State.PendingInvitations, "PendingInvitations");
            Server.State.FriendRequests = (IList<Contact>)Server.State.DeserializeObject(Server.State.FriendRequests, "FriendRequests");
        }

        private void SerializeObject(Object obj, string file)
        {
            var port = ServerIP.Split(':');
            TextWriter tw = new StreamWriter(string.Format("{0} - {1}.xml", port[1], file));
            Serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            Serializer.Serialize(tw, obj);
            //Console.WriteLine(obj + " written to file: " + obj.GetType() + ".xml");
            tw.Close();
        }

        private Object DeserializeObject(Object obj, string file)
        {
            try
            {
                var port = ServerIP.Split(':');
                TextReader tr = new StreamReader(string.Format("{0} - {1}.xml", port[1], file));
                Serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                var fileP = Serializer.Deserialize(tr);
                tr.Close();
                return fileP;
            }
            catch (FileNotFoundException)
            {

                SerializeObject(obj, file);
                Object o = DeserializeObject(obj, file);
                return o;
            }
        }

        #endregion

        #region Print Server State

        public void PrintInfo()
        {
            PrintServers();
            PrintProfile();
            PrintFriendRequests();
            PrintPendingInvitations();
            PrintContacts();
            PrintMessages();
        }

        public void PrintProfile()
        {
            Console.WriteLine("*************PROFILE**************");
            Console.WriteLine("IP: {0}", Profile.IP);
            Console.WriteLine("Username: {0}", Profile.UserName);
            Console.WriteLine("Age: {0}", Profile.Age);
            Console.WriteLine("Gender: {0}", Profile.Gender);
            Console.WriteLine("Post Seq. Number: {0}", Profile.PostSeqNumber);
            Console.WriteLine("Interests: ");
            foreach (var item in Profile.Interests)
            {
                Console.WriteLine(" {0},", item);
            }
            Console.WriteLine("**********************************");
        }

        public void PrintServers()
        {
            Console.WriteLine("*************Replics*************");
            foreach (var item in ReplicationServers)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine("**********************************");
        }

        public void PrintContacts()
        {
            Console.WriteLine("*************CONTACTS*************");
            foreach (var item in Contacts)
            {
                Console.WriteLine("Addr:{0} - UserName:{1} - MsgSeqNumber:{2}", item.IP, item.Username, item.LastMsgSeqNumber);
            }
            Console.WriteLine("**********************************");
        }

        public void PrintFriendRequests()
        {
            Console.WriteLine("***********Friend Requests********");
            foreach (var item in FriendRequests)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine("**********************************");
        }

        public void PrintPendingInvitations()
        {
            Console.WriteLine("***********Pending Invitations********");
            foreach (var item in PendingInvitations)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine("**********************************");
        }

        public void PrintMessages()
        {
            Console.WriteLine("*************MESSAGES*************");
            for (int a = 0; a < Messages.Count; a++)
            {
                if (a != Messages.Count - 1)
                {
                    Console.WriteLine("From: {0}", Messages[a].FromUserName);
                    Console.WriteLine("Time: {0}", Messages[a].Time);
                    Console.WriteLine("Sequence Number: {0}", Messages[a].SeqNumber);
                    Console.WriteLine("Post: {0}", Messages[a].Post);
                    Console.WriteLine("------------------------------");
                }
                else
                {
                    Console.WriteLine("From: {0}", Messages[a].FromUserName);
                    Console.WriteLine("Time: {0}", Messages[a].Time);
                    Console.WriteLine("Sequence Number: {0}", Messages[a].SeqNumber);
                    Console.WriteLine("Post: {0}", Messages[a].Post);
                }
            }
            Console.WriteLine("**********************************");
        }

        #endregion

    }
}