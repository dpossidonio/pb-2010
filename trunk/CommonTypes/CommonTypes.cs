﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{

    public interface IServerServer {
        void ReceiveFriendRequestOK(Contact c);
        void ReceiveFriendRequest(Contact c);
        void ReceiveMessage(Message msg);
    }

    public interface IServerClient
    {
        void Connect(string ip);
        IList<Message> GetMessages();
        IList<Contact> GetFriendsContacts();
        IList<Contact> GetFriendsRequestsContacts();
        Profile GetProfile();

        void Freeze();
        void UpdateProfile(Profile profile);
        Message Post(string message);
        void PostFriendRequest(string username, string address);
        Message RespondToFriendRequest(Contact c, bool accept);

        IList<Message> RefreshView();

        //Lookup();
    }

    public interface IClient
    {
        //Call by the new coordinator server
        void Coordinator(string IP);
        void UpdateFriendRequest(IList<Contact> FriendRequests);
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
        public BigInteger PostSeqNumber;

        public Profile()
        {
            Interests = new List<Interest>();
            PostSeqNumber = new BigInteger(0);
        }
    }

    [Serializable]
    public class Message {
        public BigInteger SeqNumber;
        public string FromUserName { get; set; }
        public string Post { get; set; }
        public DateTime Time;
    }

    [Serializable]
    public class Contact {
        public BigInteger LastMsgSeqNumber;
        public string IP { get; set; }
        public string Username { get; set; }
    }
}
