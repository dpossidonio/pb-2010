using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Server
{
    public class ChordNode
    {
        uint _id;
        IServerServer _sucessor;
        IServerServer _sucessor2;
        IServerServer _predecessor;

        public ChordNode()
        {
            _sucessor = null;
            _sucessor2 = null;
            _predecessor = null;

        }

        //geters & seters
        public uint ID { get { return _id; } set { _id = value; } }
        public IServerServer Sucessor { get { return _sucessor; } set { _sucessor = value; } }
        public IServerServer Sucessor2 { get { return _sucessor2; } set { _sucessor2 = value; } }
        public IServerServer Predecessor { get { return _predecessor; } set { _predecessor = value; } }

        //hashing functions
        private byte[] HashCreator(String hashstr)
        {
            System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1.Create();
            System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
            byte[] combined = encoder.GetBytes(hashstr);
            hash.ComputeHash(combined);
            return hash.Hash;
        }
        private uint IDCreator(String hashstr)
        {
            try
            {
                byte[] hash = HashCreator(hashstr);
                uint id = BitConverter.ToUInt32(hash, 0);
                return id;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //others
        public bool HasSucessor() { if (_sucessor == null) return false; else return true; }
        public bool HasSucessor2() { if (_sucessor2 == null) return false; else return true; }
        public bool HasPredecessor() { if (_predecessor == null) return false; else return true; }
        public void SetID(string name) { _id = IDCreator(name); }

    }
}
