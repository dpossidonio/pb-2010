using System;
using CommonTypes;
using System.Collections.Generic;

namespace Server
{
    public partial class ServerServer : MarshalByRefObject, IServerServer
    {
        /// <summary>
        /// Outside call Functions
        /// </summary>

        private IServerServer RetServerFromIp(string ip)
        {
            return (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", ip));
        }
        public void SetPredecessorData(string ip)
        {
            node.PredecessorIP = ip;
            node.PredecessorIDByName = node.Predecessor.GetServerIDName();
            node.PredecessorIDBySexAge = node.Predecessor.GetServerIDSexAge();
            node.PredecessorIDByInterest = node.Predecessor.GetServerIDInterest();
        }
        public void SetSucessorData(string ip)
        {
            node.SucessorIP = ip;
            node.SucessorIDByName = node.Sucessor.GetServerIDName();
            node.SucessorIDBySexAge = node.Sucessor.GetServerIDSexAge();
            node.SucessorIDByInterest = node.Sucessor.GetServerIDInterest();
        }
        public void SetSucessor2Data(string ip)
        {
            node.Sucessor2IP = ip;
            node.Sucessor2IDByName = node.Sucessor2.GetServerIDName();
            node.Sucessor2IDBySexAge = node.Sucessor2.GetServerIDSexAge();
            node.Sucessor2IDByInterest = node.Sucessor2.GetServerIDInterest();
        }

        public void SetSucessor(string ip) { node.Sucessor = RetServerFromIp(ip); SetSucessorData(ip); }
        public void SetSucessor2(string ip) { node.Sucessor2 = RetServerFromIp(ip); SetSucessor2Data(ip); }
        public void SetPredecessor(string ip) { node.Predecessor = RetServerFromIp(ip); SetPredecessorData(ip); }

        public string GetServerIP() { return Server.State.ServerIP; }
        public uint GetServerIDName() { return node.IDName; }
        public uint GetServerIDSexAge() { return node.IDSexAge; }
        public List<uint> GetServerIDInterest() { return node.IDInterest; }

        public string PrintSucessores()
        {
            if (node.HasSucessor() && node.HasSucessor2())
                return "Sucessor: " + node.SucessorIP + "\nSucessor2: " + node.Sucessor2IP + "\nPredecessor: " + node.PredecessorIP;
            else if (node.HasSucessor())
                return "Sucessor: " + node.SucessorIP + "\nPredecessor: " + node.PredecessorIP;
            else
                return "You are not in a RING!";
        }

        /// <summary>
        /// Hashing Functions
        /// </summary>
        public byte[] HashCreator(String hashstr)
        {
            System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1.Create();
            System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
            byte[] combined = encoder.GetBytes(hashstr);
            hash.ComputeHash(combined);
            return hash.Hash;
        }
        public uint IDCreator(String hashstr)
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


        /// <summary>
        /// INBOUND & OUTBOUND
        /// </summary>

        //tem aki buuuuuuuggggggggg
        public string Lookup(uint ID)
        {
            if (node.IDName < ID && node.SucessorIDByName >= ID) return GetServerIP();
            if (node.SucessorIDByName > ID && node.Sucessor2IDByName <= ID) return node.SucessorIP;
            return node.Sucessor2.Lookup(ID);
        }

        /// <summary>
        /// OUTBOUND
        /// </summary>

        public string ChordJoin(string ip)
        {
            if (!node.HasSucessor() && !node.HasSucessor2() && !node.HasPredecessor())
            {
                node.SetIDName(Server.State.Profile.UserName);
                node.Predecessor = RetServerFromIp(ip);

                object[] o = node.Predecessor.ChordNodeRequestingToJoin(Server.State.ServerIP);

                //custa agora fazer estes pedidos todos mas no futuro poupa trabalho
                SetPredecessorData(ip);

                node.Sucessor = RetServerFromIp((string)o[0]);
                SetSucessorData((string)o[0]);

                node.Sucessor2 = RetServerFromIp((string)o[1]);
                SetSucessor2Data((string)o[1]);

                return "Node successfully joined a ring";
            }
            else
                return "Node already in a ring!";
        }

        public string ChordLeave()
        {
            if (node.HasSucessor() && node.HasSucessor2() && node.HasPredecessor())
            {
                node.Predecessor.ChordNodeRequestingToLeave(node.Sucessor2IP);
                node.NullData();
                return "Node successfully left the ring";
            }
            else
                return "This node doesn't belong to any ring!";
        }

        /// <summary>
        /// INBOUND
        /// </summary>

        public object[] ChordNodeRequestingToJoin(string ip)
        {
            //tá implementado de forma a que o nó que pede para entrar é posto à frente do nó que já estava no anel
            var v = RetServerFromIp(ip);
            //lista de sucessores que tinha
            if (!node.HasPredecessor() && !node.HasSucessor2() && !node.HasSucessor())
            {
                node.SetIDName(Server.State.Profile.UserName);
                node.Predecessor = v;
                SetPredecessorData(ip);
                node.Sucessor = v;
                SetSucessorData(ip);
                node.Sucessor2 = RetServerFromIp(GetServerIP());
                SetSucessor2Data(GetServerIP());

                object[] aux = { GetServerIP(), ip };
                return aux;
            }

            object[] auxx = { node.SucessorIP, node.Sucessor2IP };

            //o predecessor do meu sucessor agora é o novo no que pus no anel
            if (node.HasSucessor())
            {
                node.Sucessor.SetPredecessor(ip);
                node.Sucessor.SetPredecessorData(ip);
            }

            //tenho de alterar o meu predecessor e dizer que o seu novo sucessor2 é quem eu pus no anel
            if (node.HasPredecessor())
            {
                node.Predecessor.SetSucessor2(ip);
                node.Predecessor.SetSucessor2Data(ip);
            }
            //eu tenho de alterar os meus sucessores
            node.Sucessor2 = node.Sucessor;
            SetSucessor2Data(node.SucessorIP);
            node.Sucessor = v;
            SetSucessorData(ip);

            return auxx;
        }

        public void ChordNodeRequestingToLeave(string ip)
        {
            if (node.Sucessor.GetServerIP().Equals(ip))//eu sou o sucessor so haviam 2 nos tenho que ficar com os meus dados a 0
            {
                node.NullData();
            }
            else
            {
                node.Sucessor = node.Sucessor2;
                SetSucessorData(node.Sucessor2IP);
                node.Sucessor2 = RetServerFromIp(ip);
                SetSucessor2Data(ip);
                node.Sucessor.SetPredecessor(GetServerIP());
                node.Predecessor.SetSucessor2(node.SucessorIP);
            }
        }

    }
}
