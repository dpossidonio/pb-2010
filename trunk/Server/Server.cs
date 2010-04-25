﻿using System;
using System.Collections.Generic;
using CommonTypes;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using System.Linq;

namespace Server
{
    public class Server
    {
        public static ServerState State;
        public static StateContext ReplicaState;
        public static ServerClient sc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        /// Server.exe "Address" "Num_Servers"
        /// </param>
        static void Main(string[] args)
        {
            //forma de obter o endereço ip da máquina sem ser hardcoded :P
            //System.Net.IPAddress[] a = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            //Console.WriteLine(a[2].ToString());
            string address = "";
            int num_rep = 0;
            switch (args.Length)
            {
                case 0:
                    Console.Write("Enter [IP:Port] to run: ");
                    address = Console.ReadLine();
                    Console.Write("Enter number of Servers: ");
                    num_rep = int.Parse(Console.ReadLine());
                    break;
                case 2:
                    address = args[0];
                    num_rep = int.Parse(args[1]);
                    break;
                default:
                    Console.WriteLine("Error: Invalid arguments \nServer.exe address number_of_replics");
                    Console.ReadLine();
                    System.Environment.Exit(1);
                    break;
            }
            Console.WriteLine("Welcome - PADIbook Server v1.0");
            Console.WriteLine("Running Server on: " + address);
            //constroi uma lista com os end. das replicas servers
            var rep_list = new List<string>();
            for (int i = 1; i < num_rep + 1; i++)
            {
                var a = string.Format(address.Substring(0, address.Length - 1) + "{0}", i);
                rep_list.Add(a);
            }
            rep_list.Remove(address);
            
            //init
            ReplicaState = new StateContext(new SlaveState());
            State = new ServerState(address);
            sc = new ServerClient();
            State.KnownServers = rep_list;


            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("info"))
                    ReplicaState.RequestStateInfo();
            }


        }
    }

    #region ServerState

    public class ServerState
    {
        private Profile _profile;
        private IList<Message> _messages;
        private IList<Contact> _contacts;
        //Pedidos enviados
        private IList<Contact> _friendRequests;
        //Pedidos recebidos
        private IList<Contact> _pendingInvitations;

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
        public string ServerIP { get; set; }
        private XmlSerializer Serializer;
        //REPLICAÇÂO
        public List<string> KnownServers { get; set; }

        public ServerState(string ip)
        {
            ServerIP = ip;
            _profile = new Profile();
            _messages = new List<Message>();
            _contacts = new List<Contact>();
            _friendRequests = new List<Contact>();
            _pendingInvitations = new List<Contact>();
            KnownServers = new List<string>();
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
        {
            lock (Messages)
            {
                Messages.Add(m);
                SerializeObject(Messages, "Messages");
            }
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterMessage(m));

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
            lock (Contacts)
            {
                Contacts.Add(c);
                SerializeObject(Contacts, "Contacts");
            }
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterContact(c,false));
        }

        public void UpdateProfile(Profile p)
        {
            lock (Profile)
            {
                _profile = p;
                SerializeObject(Profile, "Profile");
            }
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterProfile(p));
        }

        public void AddFriendRequest(Contact c)
        {
            lock (FriendRequests)
            {
                FriendRequests.Add(c);
                SerializeObject(FriendRequests, "FriendRequests");
            }
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterFriendRequest(c,true));
        }

        public void RemoveFriendRequest(Contact c)
        {
            var caux = FriendRequests.First(x => x.IP.Equals(c.IP));
            lock (FriendRequests)
            {
                FriendRequests.Remove(caux);
                SerializeObject(FriendRequests, "FriendRequests");
            }
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterFriendRequest(c, false));
        }

        public void AddFriendInvitation(Contact c)
        {
            lock (PendingInvitations)
            {
                PendingInvitations.Add(c);
                SerializeObject(PendingInvitations, "PendingInvitations");
            }
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterPendingInvitation(c,true));
        }

        public void RemoveFriendInvitation(Contact c)
        {
            var caux = PendingInvitations.First(x => x.IP.Equals(c.IP));
            lock (PendingInvitations)
            {
                PendingInvitations.Remove(caux);
                SerializeObject(PendingInvitations, "PendingInvitations");
            }
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterPendingInvitation(c, false));

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
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterContact(c, true));
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
            foreach (var item in KnownServers)
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
    #endregion
}
