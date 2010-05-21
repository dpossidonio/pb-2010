using System;
using CommonTypes;
using System.Collections.Generic;

namespace Server
{
    class Node
    {
        IServerServer _iserverserver;
        uint _idByName;
        uint _idBySexAge;
        List<uint> _idByInterest;
        string _ip;

        public Node()
        {
            _iserverserver = null;
            _ip = "";
        }

        public IServerServer Server { get { return _iserverserver; } set { _iserverserver = value; } }
        public uint IDByName { get { return _idByName; } set { _idByName = value; } }
        public uint IDBySexAge { get { return _idBySexAge; } set { _idBySexAge = value; } }
        public List<uint> IDByInterest { get { return _idByInterest; } set { _idByInterest = value; } }
        public string IP { get { return _ip; } set { _ip = value; } }
        public void Format()
        {
            _iserverserver = null;
            _idByInterest.Clear();
            _idByName = 0;
            _idBySexAge = 0;
            _ip = "";
        }

    }

    public class ChordNode
    {
        uint _idName;
        uint _idSexAge;
        List<uint> _idInterest;
        Node _nsucessor;
        Node _nsucessor2;
        Node _npredecessor;

        public ChordNode()
        {
            _nsucessor = new Node();
            _nsucessor2 = new Node();
            _npredecessor = new Node();
            _idName = 0;
            _idSexAge = 0;
            _idInterest = new List<uint>();
        }

        //geters & seters
        public uint IDName { get { return _idName; } set { _idName = value; } }
        public uint IDSexAge { get { return _idSexAge; } set { _idSexAge = value; } }
        public List<uint> IDInterest { get { return _idInterest; } set { _idInterest = value; } }

        public IServerServer Sucessor { get { return _nsucessor.Server; } set { _nsucessor.Server = value; } }
        public string SucessorIP { get { return _nsucessor.IP; } set { _nsucessor.IP = value; } }
        public uint SucessorIDByName { get { return _nsucessor.IDByName; } set { _nsucessor.IDByName = value; } }
        public uint SucessorIDBySexAge { get { return _nsucessor.IDBySexAge; } set { _nsucessor.IDBySexAge = value; } }
        public List<uint> SucessorIDByInterest { get { return _nsucessor.IDByInterest; } set { _nsucessor.IDByInterest = value; } }

        public IServerServer Sucessor2 { get { return _nsucessor2.Server; } set { _nsucessor2.Server = value; } }
        public string Sucessor2IP { get { return _nsucessor2.IP; } set { _nsucessor2.IP = value; } }
        public uint Sucessor2IDByName { get { return _nsucessor2.IDByName; } set { _nsucessor2.IDByName = value; } }
        public uint Sucessor2IDBySexAge { get { return _nsucessor2.IDBySexAge; } set { _nsucessor2.IDBySexAge = value; } }
        public List<uint> Sucessor2IDByInterest { get { return _nsucessor2.IDByInterest; } set { _nsucessor2.IDByInterest = value; } }

        public IServerServer Predecessor { get { return _npredecessor.Server; } set { _npredecessor.Server = value; } }
        public string PredecessorIP { get { return _npredecessor.IP; } set { _npredecessor.IP = value; } }
        public uint PredecessorIDByName { get { return _npredecessor.IDByName; } set { _npredecessor.IDByName = value; } }
        public uint PredecessorIDBySexAge { get { return _npredecessor.IDBySexAge; } set { _npredecessor.IDBySexAge = value; } }
        public List<uint> PredecessorIDByInterest { get { return _npredecessor.IDByInterest; } set { _npredecessor.IDByInterest = value; } }

        public void NullData()
        {
           _nsucessor.Format();
           _nsucessor2.Format();
           _npredecessor.Format();
        }

        public void SetIDName(string name) { _idName = IDCreator(name); }
        public void SetIDSexAge(string name) { _idSexAge = IDCreator(name); }
        public void SetIDInterest(string name) { _idInterest.Add(IDCreator(name)); }
        public void PredecessorAddInterest(uint item) { _npredecessor.IDByInterest.Add(item); }
        public void SucessorAddInterest(uint item) { _nsucessor.IDByInterest.Add(item); }
        public void Sucessor2AddInterest(uint item) { _nsucessor2.IDByInterest.Add(item); }

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
        public bool HasSucessor() { if (_nsucessor.Server == null) return false; else return true; }
        public bool HasSucessor2() { if (_nsucessor2.Server == null) return false; else return true; }
        public bool HasPredecessor() { if (_npredecessor.Server == null) return false; else return true; }


    }
}
