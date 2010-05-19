using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

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
        void ReplicateContacts(StateContext context, CommonTypes.Contact contac);
        void ReplicateFriendRequest(StateContext context, CommonTypes.Contact contact,bool b);
        void ReplicatePendingInvitation(StateContext context, CommonTypes.Contact contact,bool b);
        //fazer o set do slave
        void Commit(StateContext context);
        void SetReplica(StateContext context, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi);
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
        public void RegisterContact(CommonTypes.Contact cont) { State.ReplicateContacts(this, cont); }
        public void RegisterFriendRequest(CommonTypes.Contact contact,bool b) { State.ReplicateFriendRequest(this, contact,b); }
        public void RegisterPendingInvitation(CommonTypes.Contact contact,bool b) { State.ReplicatePendingInvitation(this, contact,b); }
        public void CommitChanges() { State.Commit(this); }
        public void InitReplica(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi) { State.SetReplica(this, p, m, c,fr,pi); }
    }

    //Implementação do estado concreto Master
    public class MasterState : IState
    {
        #region IStateMembers

        public void Info(StateContext context)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("-----------------  IM IN MASTER STATE ------------------------");
            Console.WriteLine("-------------------- {0} --------------------------",Server.State.ServerIP);
            Server.State.PrintInfo();        
        }

        public void Change(StateContext context)
        {
            context.State = new SlaveState(); 
        }

        public void AddMessage(StateContext stateContext, CommonTypes.Message msg)
        {
            Console.WriteLine("MASTER: UPDATING THE MESSAGES IN SLAVES");
            Server.sc.ServerServer.ReplicateMessage(Server.State.ReplicationServers, msg);
        }

        public void SetReplica(StateContext context, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi)
        {
            Console.WriteLine("MASTER: UPDATING ALL SLAVES CONTENT");
            Server.sc.ServerServer.SetSlave(Server.State.ReplicationServers, p, m, c,fr,pi);
        }

        public void ReplicateProfile(StateContext context, CommonTypes.Profile profile)
        {
            Server.sc.ServerServer.SetProfile(Server.State.ReplicationServers, profile);           
        }

        public void ReplicateContacts(StateContext context, CommonTypes.Contact contact)
        {
            Server.sc.ServerServer.SetContact(Server.State.ReplicationServers, contact);
        }

        public void ReplicateFriendRequest(StateContext context, CommonTypes.Contact contact,bool b)
        {
            Server.sc.ServerServer.SetFriendRequest(Server.State.ReplicationServers, contact,b);
            
        }

        public void ReplicatePendingInvitation(StateContext context, CommonTypes.Contact contact,bool b)
        {
            Server.sc.ServerServer.SetFriendInvitation(Server.State.ReplicationServers, contact,b);  
        }

        public void Commit(StateContext context)
        {
        }

        #endregion
    }

    //Implementação do estado concreto Slave
    public class SlaveState : IState
    {
        #region IState Members

        public void Info(StateContext context) 
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("-----------------  IM IN SLAVE STATE -------------------------");
            Console.WriteLine("-------------------- {0} --------------------------", Server.State.ServerIP);
            Server.State.PrintInfo();
        }

        public void Change(StateContext context)
        {
            Console.WriteLine("I WAS IN SLAVE STATE - NOW IM GOING TO MASTER STATE");
            context.State = new MasterState(); 
        }

        public void AddMessage(StateContext stateContext, CommonTypes.Message msg)
        {
            Console.WriteLine("SLAVE: ADDING THIS MSG: " + msg.Post);
            Server.State.CommitMessage(msg);
            throw new NotImplementedException();
        }

        public void SetReplica(StateContext context, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi) {
            Console.WriteLine("SLAVE: FULL Update from Master."); 
        }

        public void ReplicateProfile(StateContext context, CommonTypes.Profile profile)
        {      
            Console.WriteLine("SLAVE: Saving Profile.");
            Server.State.CommitProfile(profile);
        }

        public void ReplicateContacts(StateContext context, CommonTypes.Contact contact)
        {
            Console.WriteLine("SLAVE: Adding/Updating Contact.");
            Server.State.CommitContact(contact);
        }

        public void ReplicateFriendRequest(StateContext context, CommonTypes.Contact contact,bool b)
        {
            Console.WriteLine("SLAVE: Adding/Updating FriendRequest.");
        }

        public void ReplicatePendingInvitation(StateContext context, CommonTypes.Contact contact,bool b)
        {
            Console.WriteLine("SLAVE: Adding/Updating PendingInvitation.");
        }

        public void Commit(StateContext context)
        {
        }

        #endregion
    }

    public class UnnavailableState : IState
    {
        #region IState Members

        public void Info(StateContext context)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("-----------------  IM IN UNNAVAILAVLE STATE ------------------");
            Console.WriteLine("-------------------- {0} --------------------------", Server.State.ServerIP);
            Server.State.PrintInfo();
        }

        public void Change(StateContext context)
        {
            throw new NotImplementedException();
        }

        public void AddMessage(StateContext stateContext, CommonTypes.Message msg)
        {
            throw new ServiceNotAvailableException(1,Server.sc);
        }

        public void SetReplica(StateContext context, CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c, IList<CommonTypes.Contact> fr, IList<CommonTypes.Contact> pi)
        {
            Console.WriteLine("SLAVE: FULL Update from Master.");

        }

        public void ReplicateProfile(StateContext context, CommonTypes.Profile profile)
        {
            throw new ServiceNotAvailableException(1, Server.sc);
        }

        public void ReplicateContacts(StateContext context, CommonTypes.Contact contact)
        {
            throw new ServiceNotAvailableException(1, Server.sc);
        }

        public void ReplicateFriendRequest(StateContext context, CommonTypes.Contact contact, bool b)
        {
            throw new ServiceNotAvailableException(1, Server.sc);
        }

        public void ReplicatePendingInvitation(StateContext context, CommonTypes.Contact contact, bool b)
        {
            throw new ServiceNotAvailableException(1, Server.sc);
        }

        public void Commit(StateContext context)
        {
            throw new ServiceNotAvailableException(1, Server.sc);
        }

        #endregion
    }
}
