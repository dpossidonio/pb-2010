using System;
using CommonTypes;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
    public partial class ServerServer : MarshalByRefObject, IServerServer
    {
        public ChordNode node;

        //Threads
        private Thread trdUpdateSearchInformation;
        private Thread trdVerifySucessorLife;

        //diz respeito à procura
        Dictionary<uint, List<string>> iddSex; //informação sobre os pares idd+sexo e IP pelo qual este servidor é responsável
        Dictionary<uint, List<string>> interest; //informação sobre os pares interese e IP pelo qual este servidor é responsável
        Dictionary<uint, List<string>> repiddSex; //informação das tabelas do predecessor
        Dictionary<uint, List<string>> repinterest; //informação das tabelas do predecessor

        //devolve a referencia para o objecto remoto
        private IServerServer RetServerFromIp(string ip)
        {
            return (IServerServer)Activator.GetObject(typeof(IServerServer), string.Format("tcp://{0}/IServerServer", ip));
        }

        //set da informação relativa aos sucessores e predecessores
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

        //set da referencia relativa aos sucessores e predecessores
        public void SetSucessor(string ip) { node.Sucessor = RetServerFromIp(ip); SetSucessorData(ip); }
        public void SetSucessor2(string ip) { node.Sucessor2 = RetServerFromIp(ip); SetSucessor2Data(ip); }
        public void SetPredecessor(string ip) { node.Predecessor = RetServerFromIp(ip); SetPredecessorData(ip); }

        public string GetServerIP() { return Server.State.ServerIP; }
        public uint GetServerIDName() { return node.IDName; }
        public uint GetServerIDSexAge() { return node.IDSexAge; }
        public List<uint> GetServerIDInterest() { return node.IDInterest; }
        public string GetSucessorIP()
        {
            return node.SucessorIP;
        }

        /// <summary>
        /// Threads TODO Functions
        /// </summary>
        //Estabilização
        private void ThreadTODO_UpdateSearchInformation()
        {
            while (true)
            {
                Thread.Sleep(Constants.timeToUpdateNodesInformation);
                UpdateSearchInformation();
                //cada nó põe no seu sucessor as duas tabelas k tem
                node.Sucessor.ReplicateMyIDsOnSucessor(iddSex, interest);
            }
        }
        private void ThreadTODO_VerifySucessorLife()
        {
            while (true)
            {
                Thread.Sleep(Constants.pingsucessor);
                if (!TryPingSucessor())
                    ForceSucessorLeave();
            }
        }

        /// <summary>
        /// Printing Functions For Debug Purpose
        /// </summary>
        public string PrintSucessores()
        {
            if (node.HasSucessor() && node.HasSucessor2())
                return "My_ID : " + node.IDName +
                    "\nID_Suc:\t" + node.SucessorIDByName    + "\tSuc: " + node.SucessorIP +
                    "\nID_Su2:\t" + node.Sucessor2IDByName   + "\tSu2: " + node.Sucessor2IP +
                    "\nID_Pre:\t" + node.PredecessorIDByName + "\tPre: " + node.PredecessorIP;
            else if (node.HasSucessor())
                return "My_ID : " + node.IDName +
                    "\nID_Suc:\t" + node.SucessorIDByName + "\tSuc: " + node.SucessorIP +
                    "\nID_Pre:\t" + node.PredecessorIDByName + "\tPre: " + node.PredecessorIP;
            else
                return "You are not in a RING!";
        }
        public string PrintInfoThatNodeIsResponsable()
        {
            string s = "Meu ID\n"+ node.IDName;
            s += "\nSexo+Idade\n";
            if (iddSex.Count != 0)
            {
                foreach (var par in iddSex)
                {
                    string ips="";
                    foreach (var v in par.Value)
                        ips += string.Format("{0} ", (string)v);
                    s += string.Format("{0}, {1}\n", par.Key,ips );
                }
            }
            s += "Intereses\n";
            if (interest.Count != 0)
            {
                foreach (var par in interest)
                {
                    string ips = "";
                    foreach (var v in par.Value)
                        ips += string.Format("{0} ", (string)v);
                    s += string.Format("{0}, {1}\n", par.Key, ips);
                }
            }

            return s;
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
        /// Searching Functions
        /// </summary>
        public List<string> SearchByName(string name)
        {
            List<string> ls = new List<string>();
            uint id =IDCreator(name);
            if (id != node.IDName) //se não procurei por mim
            {
                string ip = Lookup(id);
                IServerServer iss = RetServerFromIp(ip);
                if (iss.GetServerIDName() != id)
                    ip = "User not found!";
                ls.Add(string.Format("{0}@{1}", name, ip));
                return ls;
            }
            ls.Add(string.Format("{0}@{1}", name, GetServerIP()));
            return ls;
        }
        public List<string> SearchBySexAge(string sexage)
        {
            uint id = IDCreator(sexage);
            IServerServer iss = RetServerFromIp(Lookup(id));
            return iss.GetIPsForSexAge(id);

        }
        public List<string> SearchByInterest(string interest)
        {
            uint id = IDCreator(interest);
            IServerServer iss = RetServerFromIp(Lookup(id));
            return iss.GetIPsForInterests(id);
        }

        /// <summary>
        /// INBOUND & OUTBOUND
        /// </summary>
        public string Lookup(uint ID)
        {
            //o servidor contactado ainda nao tinha um ID
            if (node.IDName == 0) node.SetIDName(Server.State.Profile.UserName);

            //anel composto por um único elemento, logo sou eu o responsável
            if (node.SucessorIDByName == 0 && node.Sucessor2IDByName == 0 && node.PredecessorIDByName == 0) return GetServerIP();

            //anel com 2 elementos, eu e outro, posso decidir quem detem a informação, ou sou eu ou é o outro
            if (node.IDName == node.Sucessor2IDByName)
            {
                //ultimo do anel e ID maior que eu -> sou eu o responsável
                if ( ID >= node.IDName && ID < node.SucessorIDByName || node.IDName > node.SucessorIDByName && ID > node.IDName )
                    return GetServerIP();
                else
                    return node.SucessorIP;
            }

            if (node.IDName == ID) return GetServerIP(); //procurei por mim
            if (ID > node.IDName &&  ID < node.SucessorIDByName) return GetServerIP(); //entre mim e o meu sucessor
            if (ID >= node.SucessorIDByName && ID < node.Sucessor2IDByName) return node.SucessorIP; //entre o meu sucessor e o meu sucessor2
            if (node.IDName > node.SucessorIDByName && ID > node.IDName) return GetServerIP(); //sou o ultimo do anel, o meu sucessor é menor k eu
            if (node.SucessorIDByName > node.Sucessor2IDByName && ID < node.Sucessor2IDByName) return node.SucessorIP; //o meu sucessor é o ultimo do anel (id maior de todos)
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

                node.Predecessor = RetServerFromIp(node.Predecessor.Lookup(node.IDName));

                object[] o = node.Predecessor.ChordNodeRequestingToJoin(Server.State.ServerIP);

                //custa agora fazer estes pedidos todos mas no futuro poupa trabalho
                SetPredecessorData(node.Predecessor.GetServerIP());

                node.Sucessor = RetServerFromIp((string)o[0]);
                SetSucessorData((string)o[0]);

                node.Sucessor2 = RetServerFromIp((string)o[1]);
                SetSucessor2Data((string)o[1]);

                //pedir ao meu sucessor as tabelas de ids k ele tem pois posso ser responsavel por algum
                object[] aux = node.Sucessor.GetReplicateMyIDsOnSucessor();
                iddSex = (Dictionary<uint, List<string>>)aux[0];
                interest = (Dictionary<uint, List<string>>)aux[1];
                //registar nos nós responsáveis a informação dos meus intereses, idd+sexo ....
                RegisterIDsOnOthers();
                return "Node successfully joined a ring";
            }
            else
                return "Node already in a ring!";
        }
        public string ChordLeave()
        {
            if (node.HasSucessor() && node.HasSucessor2() && node.HasPredecessor())
            {
                //passar o conteudo das minhas listas de ids-ips para o predecessor
                GiveMyKeyIpContentToPredecessor();
                //remover a minha informação nos outros
                DeleteIDsOnOthers();
                //por fim sair
                node.Predecessor.ChordNodeRequestingToLeave(node.Sucessor2IP);
                node.NullData();
                return "Node successfully left the ring";
            }
            else
                return "This node doesn't belong to any ring!";
        }
        public void RegisterIDsOnOthers()
        {
            //atribuição dos IDs da idade+sexo e de cada interese
            if (Server.State.Profile.Age != 0) //por default a idade vem a 0
            {
                string iddsexo = string.Format("{0}{1}", Server.State.Profile.Gender, Server.State.Profile.Age);
                node.SetIDSexAge(iddsexo);
            }
            foreach (int v in Server.State.Profile.Interests)
            {
                node.SetIDInterest(string.Format("{0}",(Interest)v));
            }
            
            //para cada ID descobrir quem é o nó responsável para pedir ao mesmo que registe a informação de que eu sou interesado por tal
            IServerServer svsv;

            if (node.IDSexAge != 0)
            {
                var s = Lookup(node.IDSexAge);
                svsv = RetServerFromIp(s);
                svsv.RegisterIDsFromOthers(node.IDSexAge, GetServerIP(), "sexidd");
            }

            if (node.IDInterest.Count != 0)
            {
                foreach (var v in node.IDInterest)
                {
                    svsv = RetServerFromIp(Lookup(v));
                    svsv.RegisterIDsFromOthers(v, GetServerIP(), "interest");
                }
            }
        }
        public void DeleteIDsOnOthers()
        {
            //para cada ID descobrir quem é o nó responsável 
            //e pedir para retirar-me do seu registo
            IServerServer svsv;

            if (node.IDSexAge != 0)
            {
                svsv = RetServerFromIp(Lookup(node.IDSexAge));
                svsv.DeleteIDsFromOthers(node.IDSexAge, GetServerIP(), "sexidd");
            }

            if (node.IDInterest.Count != 0)
            {
                foreach (var v in node.IDInterest)
                {
                    svsv = RetServerFromIp(Lookup(v));
                    svsv.DeleteIDsFromOthers(v, GetServerIP(), "interest");
                }
            }
        }
        //semelhante à anterior mas para o caso em que o meu sucessor falhou e eu removo a informação dele na rede
        public void DeleteSucessorIDsOnOthers()
        {
            IServerServer svsv;

            if (node.IDSexAge != 0)
            {
                svsv = RetServerFromIp(Lookup(node.SucessorIDBySexAge));
                svsv.DeleteIDsFromOthers(node.SucessorIDBySexAge, node.SucessorIP, "sexidd");
            }

            if (node.IDInterest.Count != 0)
            {
                foreach (var v in node.SucessorIDByInterest)
                {
                    svsv = RetServerFromIp(Lookup(v));
                    svsv.DeleteIDsFromOthers(v, node.SucessorIP, "interest");
                }
            }
        }
        public void UpdateSearchInformation()
        {
            //fora do anel nada faço
            if (!node.HasPredecessor() && !node.HasSucessor() && !node.HasSucessor2())
                return;

            List<uint> keysToDeleteAfterForeach =new List<uint> ();
            List<string> ipsToDeleteAfterForeach = new List<string>();
            IServerServer iss;

            foreach (var pair in iddSex)
            {
                var auxnode = Lookup(pair.Key);
                if (!auxnode.Equals(GetServerIP()))
                {
                    //actualizar lista de ips antes de mandar é para o caso de eu ter
                    //informação de um nó que saiu por algum motivo e eu nao fui informado
                    foreach (var v in pair.Value)
                    {
                        try
                        {
                            iss = RetServerFromIp(v);
                            iss.PingNode();
                        }
                        catch
                        {//nó nao existe
                            ipsToDeleteAfterForeach.Add(v);
                        }
                    }
                    foreach (var v in ipsToDeleteAfterForeach)
                        pair.Value.Remove(v);
                    ipsToDeleteAfterForeach.Clear();

                    //mandar lista actualizada
                    iss = RetServerFromIp(auxnode);
                    iss.AddIDsSexIdd(pair.Key, pair.Value);
                    keysToDeleteAfterForeach.Add(pair.Key);
                }
            }

            foreach (var v in keysToDeleteAfterForeach)
                iddSex.Remove(v);
            keysToDeleteAfterForeach.Clear();

            foreach (var pair in interest)
            {
                var auxnode = Lookup(pair.Key);
                if (!auxnode.Equals(GetServerIP()))
                {
                    //actualizar lista de ips antes de mandar é para o caso de eu ter
                    //informação de um nó que saiu por algum motivo e eu nao fui informado
                    foreach (var v in pair.Value)
                    {
                        try
                        {
                            iss = RetServerFromIp(v);
                            iss.PingNode();
                        }
                        catch
                        {//nó nao existe
                            ipsToDeleteAfterForeach.Add(v);
                        }
                    }
                    foreach (var v in ipsToDeleteAfterForeach)
                        pair.Value.Remove(v);
                    ipsToDeleteAfterForeach.Clear();

                    //mandar lista actualizada
                    iss = RetServerFromIp(auxnode);
                    iss.AddIDsInterest(pair.Key, pair.Value);
                    keysToDeleteAfterForeach.Add(pair.Key);
                    
                }
            }

            foreach (var v in keysToDeleteAfterForeach)
                interest.Remove(v);

        }
        public void GiveMyKeyIpContentToPredecessor()
        {
            foreach (var pair in iddSex)
            {
                node.Predecessor.AddIDsSexIdd(pair.Key, pair.Value);
            }
            foreach (var pair in interest)
            {
                node.Predecessor.AddIDsInterest(pair.Key, pair.Value);
            }
        }
        public bool TryPingSucessor()
        {
            //fora do anel nada faço
            if (!node.HasPredecessor() && !node.HasSucessor() && !node.HasSucessor2())
                return true;

            try
            {
                node.Sucessor.PingNode();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public void ForceSucessorLeave()
        {
            //eu invoco em mim proprio a saida do meu sucessor;
            ForcedChordNodeRequestingToLeave(node.Sucessor2IP);
        }

        /// <summary>
        /// INBOUND
        /// </summary>

        public object[] ChordNodeRequestingToJoin(string ip)
        {
            //tá implementado de forma a que o nó que pede para entrar é posto à frente do nó que já estava no anel
            var v = RetServerFromIp(ip);
            //não tenho sucessores ou seja nao tou num anel sou eu o anel :P
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

                //distribuir a minha informação pelo anel recem formado 10segundos dps
                ThreadPool.QueueUserWorkItem((object o) => { Thread.Sleep(10000); RegisterIDsOnOthers(); }); 
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


            //tenho de passar para o meu sucessor informação de pesquisa que detinha antes de ele entras
            //mas agora é ele o responsável
            UpdateSearchInformation();

            return auxx;
        }
        public void ChordNodeRequestingToLeave(string ip)
        {
            //eu sou o sucessor so haviam 2 nos tenho que ficar com os meus dados a 0
            if (node.SucessorIP.Equals(ip))
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
        //tive de forçar o meu sucessor a sair
        public void ForcedChordNodeRequestingToLeave(string ip)
        {
            //eu sou o sucessor so haviam 2 nos tenho que ficar com os meus dados a 0
            if (node.SucessorIP.Equals(ip))
            {
                node.NullData();
            }
            else
            {
                node.Sucessor = node.Sucessor2;
                SetSucessorData(node.Sucessor2IP);
                var s = node.Sucessor.GetSucessorIP();
                node.Sucessor2 = RetServerFromIp(s);
                SetSucessor2Data(s);
                node.Sucessor.SetPredecessor(GetServerIP());
                node.Predecessor.SetSucessor2(node.SucessorIP);
            }
        }
        
        //ID related
        public void RegisterIDsFromOthers(uint ID, string ip, string type)
        {
            if(type.Equals("sexidd"))
            {
                RegisterIDSexIdd(ID, ip);
            }

            if (type.Equals("interest"))
            {
                RegisterIDInterest(ID, ip);
            }
        }
        public void DeleteIDsFromOthers(uint ID, string ip, string type)
        {
            if (type.Equals("sexidd"))
            {
                DeleteIDSexIdd(ID, ip);
            }

            if (type.Equals("interest"))
            {
                DeleteIDInterest(ID, ip);
            }
        }
        public void RegisterIDSexIdd(uint ID,string ip)
        {
            lock (iddSex)
            {
                if (iddSex.Count == 0) //lista vazia
                {
                    List<string> ls = new List<string>();
                    ls.Add(ip);
                    iddSex.Add(ID, ls);
                }
                else //lista já com elementos ver se o que queremos inserir existe
                {
                    try
                    {
                        List<string> ls = iddSex[ID];
                        ls.Add(ip);
                        iddSex.Remove(ID);
                        iddSex.Add(ID, ls);
                    }
                    catch (Exception) //não existia temos de criar
                    {
                        List<string> ls = new List<string>();
                        ls.Add(ip);
                        iddSex.Add(ID, ls);
                    }
                }
            }
        }
        public void RegisterIDInterest(uint ID, string ip)
        {
            lock (interest)
            {
                if (interest.Count == 0) //lista vazia
                {
                    List<string> ls = new List<string>();
                    ls.Add(ip);
                    interest.Add(ID, ls);
                }
                else //lista já com elementos ver se o que queremos inserir existe
                {
                    try
                    {
                        List<string> ls = interest[ID];
                        ls.Add(ip);
                        interest.Remove(ID);
                        interest.Add(ID, ls);
                    }
                    catch (Exception) //não existia temos de criar
                    {
                        List<string> ls = new List<string>();
                        ls.Add(ip);
                        interest.Add(ID, ls);
                    }
                }
            }
        }
        public void DeleteIDSexIdd(uint ID, string ip)
        {

            try
            {
                List<string> ls = iddSex[ID];
                ls.Remove(ip);
                iddSex.Add(ID, ls);
            }
            catch (Exception) //não existia ignoramos
            {}
            
        }
        public void DeleteIDInterest(uint ID, string ip)
        {

            try
            {
                List<string> ls = interest[ID];
                ls.Remove(ip);
                interest.Add(ID, ls);
            }
            catch (Exception) //não existia ignoramos
            { }
            
        }
        public void AddIDsSexIdd(uint ID, List<string> list)
        {
            try
            {
                List<string> ls = iddSex[ID];
                foreach (var v in list)
                    ls.Add(v);
                iddSex.Remove(ID);
                iddSex.Add(ID, ls);
            }
            catch (Exception) //não existia
            {
                iddSex.Add(ID, list);
            }
        }
        public void AddIDsInterest(uint ID, List<string> list)
        {
            try
            {
                List<string> ls = interest[ID];
                foreach (var v in list)
                    ls.Add(v);
                interest.Remove(ID);
                interest.Add(ID, ls);
            }
            catch (Exception) //não existia
            {
                interest.Add(ID, list);
            }
        }

        public List<string> GetIPsForSexAge(uint id)
        {
            try
            {
                return iddSex[id];
            }
            catch (Exception) //não existia
            {
                return new List<string>();
            }
        }
        public List<string> GetIPsForInterests(uint id)
        {
            try
            {
                return interest[id];
            }
            catch (Exception) //não existia
            {
                return new List<string>();
            }
        }

        public void PingNode()
        {
            return;
        }

        public void ReplicateMyIDsOnSucessor(Dictionary<uint, List<string>> sexage, Dictionary<uint, List<string>> interests)
        {
            repiddSex = sexage;
            repinterest = interest;
        }
        public object[] GetReplicateMyIDsOnSucessor()
        {
            object[] o = {repiddSex ,repinterest };
            return o;
        }
    }
}
