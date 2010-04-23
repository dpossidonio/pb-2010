using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{

    public interface IServerServer {
        void ReceiveFriendRequestOK(Contact c);
        void ReceiveFriendRequest(Contact c);
        void ReceiveMessage(Message msg);
        IList<Message> RequestMessages(int lastSeqNumber);
  
        void UpdateSlave(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi);
        void UpdateMessages(Message msg);
        void UpdateProfile(Profile p);
        void UpdateContacts(Contact c);
        void UpdateFriendRequest(Contact c);
        void UpdatePendingInvitation(Contact c);
    }

    public interface IServerClient
    {
        void Connect(string ip);
        IList<Message> GetMessages();
        IList<Contact> GetFriendsContacts();
        IList<Contact> GetPendingInvitations();
        Profile GetProfile();

        void Freeze();
        void UpdateProfile(Profile profile);
        Message Post(string message);
        void PostFriendRequest(string address);
        Message RespondToFriendRequest(Contact c, bool accept);

        void RefreshView();

        //Lookup();
    }

    public interface IClient
    {
        //Call by the new coordinator server
        void Coordinator(string IP);
        void UpdateFriendInvitation(IList<Contact> FriendRequests);
        void UpdatePosts(IList<Message> NewPosts);
        void UpdateFriends(Contact Friend);
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

        public override string ToString() {
            return string.Format("{0}  ,  {1}", Username, IP);
        }
    }
}
