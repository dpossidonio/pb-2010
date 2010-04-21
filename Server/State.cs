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
        public void InitReplica(CommonTypes.Profile p, IList<CommonTypes.Message> m, IList<CommonTypes.Contact> c) { State.SetReplica(this, p, m, c); }
    }

    //Implementação do estado concreto Master
    public class MasterState : IState
    {
        public void Info(StateContext context)
        {
            Console.WriteLine("IM IN MASTER STATE");
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
    }

    //Implementação do estado concreto Slave
    public class SlaveState : IState
    {
        public void Info(StateContext context) 
        {
            Console.WriteLine("IM IN SLAVE STATE");
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

    }
}
