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

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome - PADIbook Server v1.0");

            //forma de obter o endereço ip da máquina sem ser hardcoded :P
            //System.Net.IPAddress[] a = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            //Console.WriteLine(a[2].ToString());
            /*
            //REPLICAÇÂO
            Console.Write("Do you know replicas? Insert there addresses if so: 127.0.0.1:");
            var know = Console.ReadLine();

            var localIP = "127.0.0.1:";
            Console.Write("Enter Port to run: ");
            var porto = Console.ReadLine();
            var ip = string.Format(localIP + porto);
            Console.WriteLine("Running Server on: " + ip);

                //REPLICAÇÂO
            ReplicaState = new StateContext(new SlaveState());
            
            State = new ServerState(ip);
            sc = new ServerClient();

            if (!know.Equals(""))
                State.KnownServers.Add(string.Format(localIP + know));


            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("info"))
                    ReplicaState.RequestStateInfo();
            }
             */
            var localIP = "127.0.0.1:";
            int ms = 0;
            
            
            Console.Write("Insert the port for ip 127.0.0.1:");
            var porto = Console.ReadLine();
            
            try
            {
                ms = Convert.ToInt32(porto) % 10;
            }
            catch(FormatException)
            {
                Console.WriteLine("Invalid port");
            }

            var ip = string.Format(localIP + porto);
            
            //Ligar Master Server ou Slave Server
            switch (ms)
            {
                
                case 1:
                    Console.WriteLine("Running Server on: " + ip);
                    //REPLICAÇÂO
                    ReplicaState = new StateContext(new SlaveState());

                    State = new ServerState(ip);
                    sc = new ServerClient();
                    var aux = Convert.ToInt32(porto);
                    
                    var slave1 = aux + 1;
                    var slave2 = aux + 2;
                    State.KnownServers.Add(string.Format(localIP + slave1.ToString()));
                    State.KnownServers.Add(string.Format(localIP + slave2.ToString()));
                    Console.WriteLine("Server automatically knows the following Servers:");
                    Console.WriteLine(localIP + slave1);
                    Console.WriteLine(localIP + slave2);
                    break;
                case 2:
                    Console.WriteLine("Running Server on: " + ip);
                    //REPLICAÇÂO
                    ReplicaState = new StateContext(new SlaveState());

                    State = new ServerState(ip);
                    sc = new ServerClient();
                    var aux2 = Convert.ToInt32(porto);

                    var slave3 = aux2 - 1;
                    var slave4 = aux2 + 1;
                    State.KnownServers.Add(string.Format(localIP + slave3.ToString()));
                    State.KnownServers.Add(string.Format(localIP + slave4.ToString()));
                    Console.WriteLine("Server automatically knows the following Servers:");
                    Console.WriteLine(localIP + slave3);
                    Console.WriteLine(localIP + slave4);
                    break;
                case 3:
                    Console.WriteLine("Running Server on: " + ip);
                    //REPLICAÇÂO
                    ReplicaState = new StateContext(new SlaveState());

                    State = new ServerState(ip);
                    sc = new ServerClient();
                    var aux3 = Convert.ToInt32(porto);

                    var slave5 = aux3 - 1;
                    var slave6 = aux3 - 2;
                    State.KnownServers.Add(string.Format(localIP + slave5.ToString()));
                    State.KnownServers.Add(string.Format(localIP + slave6.ToString()));
                    Console.WriteLine("Server automatically knows the following Servers:");
                    Console.WriteLine(localIP + slave5);
                    Console.WriteLine(localIP + slave6);
                    break;
                default:
                    break;

            }

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
                    SerializeObject(Profile,"Profile");
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
                    SerializeObject(Messages,"Messages");
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
                    SerializeObject(Contacts,"Contacts");
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
                    SerializeObject(FriendRequests,"FriendRequests");
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

        public void PrintInfo()
        {
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

        public void PrintContacts()
        {
            Console.WriteLine("*************CONTACTS*************");
            foreach (var item in Contacts)
            {
                Console.WriteLine(item.ToString());
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
                SerializeObject(Messages,"Messages");
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
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterContact(c));
        }

        public void UpdateProfile(Profile p)
        {
            lock (Profile)
            {
                _profile = p;
                SerializeObject(Profile,"Profile");
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
          ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterFriendRequest(c));
        }

        public void AddFriendInvitation(Contact c)
        {
            lock (PendingInvitations)
            {
                PendingInvitations.Add(c);
                SerializeObject(PendingInvitations, "PendingInvitations");
            }
            //Replicação
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterPendingInvitation(c));
        }

        public void RemoveFriendRequest(Contact c)
        {
            var caux = FriendRequests.First(x => x.IP.Equals(c.IP));
            lock (FriendRequests)
            {
                FriendRequests.Remove(caux);
                SerializeObject(FriendRequests, "FriendRequests");
            }
        }

        public void RemoveFriendInvitation(Contact c)
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
            lock (Contacts)
            {
                c.LastMsgSeqNumber = seqNumber;
                SerializeObject(Contacts, "Contacts");
            }
        }

        public void DeserializeState()
        {
            Server.State.Contacts = (IList<Contact>)Server.State.DeserializeObject(Server.State.Contacts, "Contacts");
            Server.State.Messages = (IList<Message>)Server.State.DeserializeObject(Server.State.Messages, "Messages");
            Server.State.UpdateProfile((Profile)Server.State.DeserializeObject(Server.State.Profile,"Profile"));
            Server.State.PendingInvitations = (IList<Contact>)Server.State.DeserializeObject(Server.State.PendingInvitations, "PendingInvitations");
            Server.State.FriendRequests = (IList<Contact>)Server.State.DeserializeObject(Server.State.FriendRequests, "FriendRequests");
        }

        private void SerializeObject(Object obj,string file)
        {
            var port = ServerIP.Split(':');
            TextWriter tw = new StreamWriter(string.Format("{0} - {1}.xml", port[1], file));
            Serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            Serializer.Serialize(tw, obj);
            //Console.WriteLine(obj + " written to file: " + obj.GetType() + ".xml");
            tw.Close();
        }

        private Object DeserializeObject(Object obj,string file)
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

                SerializeObject(obj,file);
                Object o = DeserializeObject(obj,file);
                return o;
            }
        }
    }
    #endregion
}
