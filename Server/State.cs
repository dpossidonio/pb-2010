using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public interface IState
    {
        //informação sobre o estado em que se encontra o servidor, pode ser consultado inserindo info na consola
        void Info(StateContext context);
        //mudar servidor de master para slave e vice-versa
        void Change(StateContext context);
        //adicionar mensagem
        void AddMessage(StateContext context, CommonTypes.Message msg);
        //regista profile
        void ReplicateProfile(StateContext context, CommonTypes.Profile profile);
        void ReplicateContacts(StateContext context, CommonTypes.Contact contact);
        //fazer o set do slave
        void SetReplica(StateContext context, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c);
    }

    public class StateContext
    {
        //variaveis do state
        public IState State { get; set; }
        
        //contrutor tem de ser sempre chamado com estado concreto com que o servidor vai ser lançado
        public StateContext(IState state) { State = state; }

        public void RequestStateInfo() { State.Info(this); }
        public void ChangeState() { State.Change(this); }
        public void RegisterMessage(CommonTypes.Message msg) { State.AddMessage(this, msg); }
        public void RegisterProfile(CommonTypes.Profile profile) { State.ReplicateProfile(this, profile); }
        public void RegisterContact(CommonTypes.Contact contact) { State.ReplicateContacts(this, contact); }
        public void InitReplica(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c) { State.SetReplica(this, p, m, c); }
    }

    //Implementação do estado concreto Master
    public class MasterState : IState
    {
        #region IStateMembers

        public void Info(StateContext context)
        {
            Console.WriteLine("IM IN MASTER STATE");
            Server.State.PrintInfo();
           
        }

        public void Change(StateContext context)
        {
            context.State = new SlaveState(); 
        }

        public void AddMessage(StateContext stateContext, CommonTypes.Message msg)
        {
            Console.WriteLine("MASTER: UPDATING THE MESSAGES IN SLAVES");
            Server.sc.ServerServer.ReplicateMessage(Server.State.KnownServers, msg);
        }

        public void SetReplica(StateContext context, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c)
        {
            Console.WriteLine("MASTER: UPDATING ALL SLAVES CONTENT");
            Server.sc.ServerServer.SetSlave(Server.State.KnownServers, p, m, c);
        }

        public void ReplicateProfile(StateContext context, CommonTypes.Profile profile)
        {
            Server.sc.ServerServer.SetProfile(Server.State.KnownServers, profile);
            
        }

        public void ReplicateContacts(StateContext context, CommonTypes.Contact contact)
        {
            Server.sc.ServerServer.SetContact(Server.State.KnownServers, contact);
        }

        #endregion
    }

    //Implementação do estado concreto Slave
    public class SlaveState : IState
    {
        #region IState Members

        public void Info(StateContext context) 
        {
            Console.WriteLine("IM IN SLAVE STATE");
            Server.State.PrintInfo();
        }

        public void Change(StateContext context)
        {
            Console.WriteLine("I WAS IN SLAVE STATE - NOW IM GOING TO MASTER STATE");
            context.State = new MasterState(); 
        }

        //cada vez que esta funcção é chamada esta a meter no profile do cliente o numero de seq da mensagem em kestão? 
        //a mensagem pode nao ter origem nele?
        public void AddMessage(StateContext stateContext, CommonTypes.Message msg)
        {
            Console.WriteLine("SLAVE: ADDING THIS MSG: " + msg.Post);


            //Server.State.Profile.PostSeqNumber = msg.SeqNumber;
            ////Serializa as mensagens
            //Server.State.AddMessage(msg);

            ////Actualiza no profile o numero de sequencia dos seus posts
            //Server.State.Profile = Server.State.Profile;
            
        }

        public void SetReplica(StateContext context, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c) { }


        public void ReplicateProfile(StateContext context, CommonTypes.Profile profile)
        {
            //Server.State.UpdateProfile(Server.State.Profile);         
            Console.WriteLine("SLAVE: Saving Profile: ");
        }

        public void ReplicateContacts(StateContext context, CommonTypes.Contact contact)
        {
            Console.WriteLine("SLAVE: Adding Contact: ");
        }

        #endregion
    }
}
