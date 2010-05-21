using System;
using System.Collections.Generic;

namespace CommonTypes
{

    public interface IServerServer {
        void ReceiveFriendRequestOK(Contact c);
        void ReceiveFriendRequest(Contact c);
        void ReceiveMessage(Message msg);
        IList<Message> RequestMessages(int lastSeqNumber);
  
        //Replicação
        void Ping();
        void StatusRequest(string ip);
        void UpdateSlave(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c,
            IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi, long server_version_id, List<string> replicas);
        void UpdateMessages(Message msg);
        void UpdateProfile(Profile p);
        void UpdateContacts(Contact c);
        void UpdateFriendRequest(Contact c,bool b);
        void UpdatePendingInvitation(Contact c,bool b);

        //ChordFunctions
        object[] ChordNodeRequestingToJoin(string ip);
        void ChordNodeRequestingToLeave(string ips2);

        void SetSucessor(string iss);
        void SetSucessor2(string iss);
        void SetPredecessor(string iss);

        void SetSucessorData(string ip);
        void SetSucessor2Data(string ip);
        void SetPredecessorData(string ip);

        string GetServerIP();
        uint GetServerIDName();
        uint GetServerIDSexAge();
        List<uint> GetServerIDInterest();

        string Lookup(uint id);
    }

    public interface IServerClient
    {
        void Connect(string ip);
        IList<Message> GetMessages();
        IList<Contact> GetFriendsContacts();
        IList<Contact> GetPendingInvitations();
        Profile GetProfile();

        void UpdateProfile(Profile profile);
        Message Post(string message);
        void PostFriendRequest(string address);
        Message RespondToFriendRequest(Contact c, bool accept);
        void RefreshView();

        //Search(string campo);
    }

    public interface IClient
    {
        void UpdateFriendInvitation(IList<Contact> FriendRequests);
        void UpdatePosts(IList<Message> NewPosts);
        void UpdateFriends(Contact Friend);
        void ServiceAvailable(IList<string> servers);
    }

    public enum Interest 
    { 
        Cars=1,Comics,Finance,Games,Hobbies,Jobs,Literature,Life,Medicine,
        Movies,Music,Nature,Painting,Personal,Politics,Religion,Science,
        Sports,Travel
    }

    public enum Gender 
    {
        Male = 0, Female 
    }

    [Serializable]
    public class Profile {

        public string IP { get; set; }
        public string UserName { get; set; }
        public Gender Gender { get; set; }
        public int Age { get; set; }
        public List<Interest> Interests { get; set; }
        public int PostSeqNumber { get; set; }

        public Profile()
        {
            Interests = new List<Interest>();
            PostSeqNumber = 0;
            UserName = "";
        }
    }

    [Serializable]
    public class Message {
        public int SeqNumber { get; set; }
        public string FromUserName { get; set; }
        public string Post { get; set; }
        public DateTime Time;
    }

    [Serializable]
    public class Contact {
        public int LastMsgSeqNumber { get; set; }
        public string IP { get; set; }
        public string Username { get; set; }
        public bool IsOnLine { get; set; }
        public override string ToString() {
            return string.Format("{0}  ,  {1}", Username, IP);
        }
    }

    /// <summary>
    ///  Exception lanched to CLIENT when que Quorum has only 1 Server
    /// </summary>
    [Serializable]
    public class ServiceNotAvailableException : ApplicationException
    {
        public int Number_Of_Servers;
        public IServerClient mo;  //proprio objecto k lançou a excepçao

        public ServiceNotAvailableException(int c, IServerClient mo)
        {
            Number_Of_Servers = c;
            this.mo = mo;
        }

        public ServiceNotAvailableException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            Number_Of_Servers = info.GetInt32("numberOfAvailableServers");
            mo = (IServerClient)info.GetValue("mo", typeof(IServerClient));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("numberOfAvailableServers", Number_Of_Servers);
            info.AddValue("mo", mo);
        }
    }

    public class NoServersAvailableException : Exception {
    
    }
}
