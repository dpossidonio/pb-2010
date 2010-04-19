using System;
using System.Collections.Generic;
using CommonTypes;
using System.IO;

namespace Server
{
    public class Server
    {
        public static ServerState State;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome - PADIbook Server");

            //forma de obter o endereço ip da máquina sem ser hardcoded :P
            //System.Net.IPAddress[] a = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            //Console.WriteLine(a[2].ToString());

            var localIP = "127.0.0.1:";
            Console.Write("Enter Port to run: ");
            var porto = Console.ReadLine();
            var ip = string.Format(localIP + porto);
            Console.WriteLine("Running Server on: " + ip);
            State = new ServerState(ip);
            var sc = new ServerClient();

            while (true)
            {
                Console.ReadLine();
            }
        }
    }

    #region ServerState

    public class ServerState
    {
        public string ServerIP { get; set; }
        public Profile Profile { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<Contact> Contacts { get; set; }
        public IList<Contact> FriendRequests { get; set; }
        System.Xml.Serialization.XmlSerializer x;

        public ServerState(string ip)
        {
            ServerIP = ip;
            Profile = new Profile();
            Messages = new List<Message>();
            Contacts = new List<Contact>();
            FriendRequests = new List<Contact>();
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

        //Esta função faz sempre a mesma coisa - Singleton?
        public Contact MakeContact() {
            var myContact = new Contact();
            //Conhece apenas o endereço do servidor do client
            myContact.IP = Server.State.ServerIP;
            myContact.Username = Server.State.Profile.UserName;
            return myContact;
        }

        //TODO - serializar a class
        public void SerializeObject(Object obj)
        {
            var port = ServerIP.Split(':');
            TextWriter tw = new StreamWriter(port[1] + obj + ".xml");
            x = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            x.Serialize(tw, obj);
            //Console.WriteLine(obj + " written to file: " + obj.GetType() + ".xml");
            Console.WriteLine("Server: event saved to file.");
            tw.Close();
        }

        public Object DeserializeObject(Object obj)
        {
            try
            {
                var port = ServerIP.Split(':');
                TextReader tr = new StreamReader(port[1] + obj + ".xml");
                x = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                var fileP = x.Deserialize(tr);
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
