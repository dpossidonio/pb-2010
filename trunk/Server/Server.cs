using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;

namespace Server
{
    public class Server : MarshalByRefObject, IServerClient
    {
        public UIServer ServerForm { get; set; }

        public Profile Profile { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<Contact> Contacts { get; set; }
        public IList<Contact> FriendRequests { get; set; }

        public Server(UIServer Form)
        {
            ServerForm = Form;
            Profile = new Profile();
            Messages = new List<Message>();
            Contacts = new List<Contact>();
            FriendRequests = new List<Contact>();

            //Test
            var p = new Profile();

            p.UserName = "David";
            p.Age = 25;
            p.Interests.Add(Interest.Movies);
            p.Interests.Add(Interest.Science);
            p.Gender = Gender.Male;
            var c = new Contact();
            c.IP = "127.0.0.1:1234";
            Contacts.Add(c);
            Create(p);
            Init();
        }


        public void Init()
        {
            //Profile Load
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(Profile.GetType());
            try
            {
                TextReader tr = new StreamReader("PADIdatabase.xml");
                Profile = (Profile)x.Deserialize(tr);
                tr.Close();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Could not locate database!");
            }

            //Load Messages
            //...
        }

        #region IServerClient Members

        public void Create(Profile p)
        {
            Profile = p;
            TextWriter tw = new StreamWriter("PADIdatabase.xml");
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(Profile.GetType());
            x.Serialize(tw, Profile);
            Console.WriteLine("object written to file");
            Console.ReadLine();
            tw.Close();
        }

        public IList<Message> Connect(string ClientIP)
        {
            var res = new List<Message>();
            var msg = new Message();
            msg.FromUserName = "David";
            msg.Post = "Ola sou eu outra vez desculpa o incómodo.";
            msg.Time = DateTime.Now;
            res.Add(msg);

            return res;
        }

        public IList<Contact> GetContacts()
        {
            throw new NotImplementedException();
        }

        public void Freeze()
        {
            throw new NotImplementedException();
        }

        public void Post(string message)
        {
            var m = new Message();
            m.FromUserName = "Xoxas";
            m.Post = message;
            ServerForm.ServerServer.Send(m);
        }

        public void FriendRequest(string address)
        {

        }

        public void RespondToFriendRequest(string username, bool accept)
        {
            throw new NotImplementedException();
        }

        public IList<Message> RefreshView()
        {
            throw new NotImplementedException();
        }

        public void UpdateProfile(Profile profil)
        {
            Profile = profil;
        }

        public Profile GetProfile()
        {
            return Profile;
        }

        #endregion



        public delegate void RemoteAsyncDelegate(Message msg);

        // This is the call that the AsyncCallBack delegate will reference.
        public static void OurRemoteAsyncCallBack(IAsyncResult ar)
        {
            // Alternative 1: Use the callback to get the return value
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("Curreu tudo bem pah");

            return;
        }

        private void Send(Message msg)
        {

            foreach (var item in Contacts)
            {
                var obj = (IServerServer)Activator.GetObject(
                typeof(IServerServer),
                string.Format("tcp://{0}/IServerServer",item.IP));

                try
                {
                   // obj.SendMessage(new Message());
                    AsyncCallback RemoteCallback = new AsyncCallback(Server.OurRemoteAsyncCallBack);
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.SendMessage);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(msg,RemoteCallback,null);
                }
                catch (SocketException) { }

            }

        }
    }

    public class ServerServer : Server, IServerServer
    {
        public ServerServer(UIServer ServerForm) :
            base(ServerForm)
        {}

        #region IServerServer Members

        public void SendFriendRequest(Contact Request)
        {
            FriendRequests.Add(Request);
            //TODO
        }

        public void SendMessage(Message msg)
        {
            ServerForm.UpdateLog("Recebi:"+msg.Post+"\r\n");
            lock (Messages)
            {
                Messages.Add(msg);
            }
        }

        #endregion
    }
}
