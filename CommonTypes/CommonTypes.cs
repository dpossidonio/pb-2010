﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IServerServer {
        void ReceiveFriendRequest(Contact Request);
        void ReceiveMessage(Message msg);
    }
    public interface IServerClient
    {
        IList<Message> Connect(string ip);
        IList<Contact> GetContacts();
        Profile GetProfile();
        void Freeze();

        void UpdateProfile(Profile profile);
        void Post(string message);
        void FriendRequest(string address);
        void RespondToFriendRequest(string username,bool accept);

        //Lookup();
    }

    public interface IClient
    {
        //Call by the new coordinator server
        void Coordinator(string IP);
        void FriendRequest(IList<Contact> FriendRequests);
        IList<Message> RefreshView();
    }

    public enum Interest { Cars=1,Comics,Finance,Games,Hobbies,Jobs,Literature,
        Life,Medicine,Movies,Music,Nature,Painting,Personal,Politics,Religion,Science,Sports,Travel}
    public enum Gender { Male = 0, Female }

    [Serializable]
    public class Profile {

        public string IP { get; set; }
        public string UserName { get; set; }
        public Gender Gender { get; set; }
        public int Age { get; set; }
        public List<Interest> Interests { get; set; }

        public Profile()
        {
            Interests = new List<Interest>();
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