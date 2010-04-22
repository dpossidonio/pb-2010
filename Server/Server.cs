using System;
using System.Collections.Generic;
using CommonTypes;
using System.IO;
using System.Threading;

namespace Server
{
    public class Server
    {
        public static ServerState State;
        public static StateContext ReplicaState;
        public static ServerClient sc;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome - PADIbook Server");

            //forma de obter o endereço ip da máquina sem ser hardcoded :P
            //System.Net.IPAddress[] a = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            //Console.WriteLine(a[2].ToString());

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
        }
    }

    #region ServerState

    public class ServerState
    {
        private Profile _profile;
        private IList<Message> _messages;
        private IList<Contact> _contacts;
        private IList<Contact> _friendRequests;
        public Profile Profile
        {
            get { return _profile; }
            set
            {
                lock (Profile)
                {
                    _profile = value;
                    SerializeObject(Profile);
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
                    SerializeObject(Messages);
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
                    SerializeObject(Contacts);
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
                    SerializeObject(FriendRequests);
                }
            }
        }
        public string ServerIP { get; set; }
        //REPLICAÇÂO
        public List<string> KnownServers { get; set; }

        System.Xml.Serialization.XmlSerializer Serializer;

        public ServerState(string ip)
        {
            ServerIP = ip;
            _profile = new Profile();
            _messages = new List<Message>();
            _contacts = new List<Contact>();
            _friendRequests = new List<Contact>();
            KnownServers = new List<string>();
        }

        public void PrintInfo()
        {
            PrintProfile();
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

        public void AddMessage(Message m)
        {
            lock (Messages)
            {
                Messages.Add(m);
                SerializeObject(Messages);
            }
                //Replicação
                ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterMessage(m));
            
        }

        public void AddContact(Contact c)
        {
            lock (Contacts)
            {
                Contacts.Add(c);
                SerializeObject(Contacts);
            }
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterContact(c));
        }

        public void UpdateProfile(Profile p)
        {
            lock (Profile)
            {
                _profile = p;
                SerializeObject(Profile);
            }
            ThreadPool.QueueUserWorkItem((object o) => Server.ReplicaState.RegisterProfile(p));          
        }

        public void AddFriendRequest(Contact c)
        {
            lock (FriendRequests)
            {
                FriendRequests.Add(c);
                SerializeObject(FriendRequests);
            }
        }

        public void RemoveFriendRequest(Contact c)
        {
            lock (FriendRequests)
            {
                FriendRequests.Remove(c);
                SerializeObject(FriendRequests);
            }
        }

        public void UpdateSeqNumber(Contact c, int seqNumber)
        {
            lock (Contacts)
            {
                c.LastMsgSeqNumber = seqNumber;
                SerializeObject(Contacts);
            }
        }

        public Message MakeMessage(string msg)
        {
            var m = new Message();
            m.FromUserName = this.Profile.UserName;
            m.Post = msg;
            m.Time = DateTime.Now;
            m.SeqNumber = this.Profile.PostSeqNumber++;
            return m;
        }

        public Contact MakeContact()
        {
            var myContact = new Contact();
            //Conhece apenas o endereço do servidor do client
            myContact.IP = Server.State.ServerIP;
            myContact.Username = Server.State.Profile.UserName;
            //Enviar o numero de sequencia da ultima mensagem? ou 0 para o amigo pedir todas os post's
            myContact.LastMsgSeqNumber = Server.State.Profile.PostSeqNumber - 1;
            return myContact;
        }

        public void DeserializeState()
        {
            Server.State.Contacts = (IList<Contact>)Server.State.DeserializeObject(Server.State.Contacts);
            Server.State.Messages = (IList<Message>)Server.State.DeserializeObject(Server.State.Messages);
            Server.State.UpdateProfile((Profile)Server.State.DeserializeObject(Server.State.Profile));
        }

        private void SerializeObject(Object obj)
        {
            var port = ServerIP.Split(':');
            TextWriter tw = new StreamWriter(port[1] + obj + ".xml");
            Serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            Serializer.Serialize(tw, obj);
            //Console.WriteLine(obj + " written to file: " + obj.GetType() + ".xml");
            tw.Close();
        }

        private Object DeserializeObject(Object obj)
        {
            try
            {
                var port = ServerIP.Split(':');
                TextReader tr = new StreamReader(port[1] + obj + ".xml");
                Serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                var fileP = Serializer.Deserialize(tr);
                tr.Close();
                return fileP;
            }
            catch (FileNotFoundException)
            {

                SerializeObject(obj);
                Object o = DeserializeObject(obj);
                return o;
            }
        }
    }
    #endregion
}
