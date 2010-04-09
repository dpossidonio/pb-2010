using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.IO;

namespace Server
{
    public class Server : MarshalByRefObject, IServer
    {
        public UIServer ServerForm { get; set; }
        public Profile Profile { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<Profile> Contacts { get; set; }

        public Server(UIServer Form)
        {
            ServerForm = Form;
            Profile = new Profile();

            //Test
            var p = new Profile();
            p.UserName = "David";
            p.Age = 25;
            p.Interests.Add(Interest.Movies);
            p.Interests.Add(Interest.Science);
            p.Gender = Gender.Male;
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

        #region IServer Members

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

        public IList<Profile> GetContacts()
        {
            throw new NotImplementedException();
        }

        public void Freeze()
        {
            throw new NotImplementedException();
        }

        public void Post(string message)
        {
            throw new NotImplementedException();
        }

        public void FriendRequest(string address)
        {
            throw new NotImplementedException();
        }

        public void RespondToFriendRequest(string username)
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
    }
}
