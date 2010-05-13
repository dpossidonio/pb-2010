using System;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
    public class Server
    {
        public static ServerState State;
        public static StateContext ReplicaState;
        public static ServerClient sc;

        public static ChordNode node;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        /// Server.exe "Address" "Num_Servers"
        /// </param>
        static void Main(string[] args)
        {
            //forma de obter o endereço ip da máquina sem ser hardcoded :P
            //System.Net.IPAddress[] a = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            //Console.WriteLine(a[2].ToString());
            string address = "";
            int num_rep = 0;
            switch (args.Length)
            {
                case 0:
                    //paiva-alterei isso para fazer debug mais rapido é so apagar isso e descomentar o que está em baixo.
                    Console.Write("Enter [Port] to run: ");
                    address = "127.0.0.1:" + Console.ReadLine();
                    num_rep = 0;
                    //Console.Write("Enter [IP:Port] to run: ");
                    //address = Console.ReadLine();
                    //Console.Write("Enter number of Servers: ");
                    //num_rep = int.Parse(Console.ReadLine());
                    break;
                case 2:
                    address = args[0];
                    num_rep = int.Parse(args[1]);
                    break;
                default:
                    Console.WriteLine("Error: Invalid arguments \nServer.exe address number_of_replics");
                    Console.ReadLine();
                    System.Environment.Exit(1);
                    break;
            }
            Console.WriteLine("Welcome - PADIbook Server v1.0");
            Console.WriteLine("Running Server on: " + address);
            //constroi uma lista com os end. das replicas servers
            var rep_list = new List<string>();
            for (int i = 1; i < num_rep + 1; i++)
            {
                var a = string.Format(address.Substring(0, address.Length - 1) + "{0}", i);
                rep_list.Add(a);
            }
            rep_list.Remove(address);

            //init
            ReplicaState = new StateContext(new SlaveState());
            State = new ServerState(address);
            sc = new ServerClient();
            State.KnownServers = rep_list;

            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("info"))
                    ReplicaState.RequestStateInfo();

                //chord
                if (input.Equals("join"))
                {
                    Console.Write("Input ip: -porto in debug- ");
                    input = "127.0.0.1:" + Console.ReadLine();
                    Console.WriteLine("Going to join a ChordRing! at {0}", input);
                    ThreadPool.QueueUserWorkItem((object o) => Console.WriteLine(sc.ServerServer.ChordJoin(input)));
                }

                if (input.Equals("leave"))
                {
                    Console.WriteLine("Going to leave the ChordRing!");
                    ThreadPool.QueueUserWorkItem((object o) => Console.WriteLine(sc.ServerServer.ChordLeave()));
                }

                if (input.Equals("ring"))
                    Console.WriteLine(sc.ServerServer.PrintSucessores());

                if (input.Equals("lookup"))
                {
                    Console.Write("Input name to search: ");
                    input = Console.ReadLine();
                    var s = sc.ServerServer.Lookup(sc.ServerServer.IDCreator(input));
                    Console.WriteLine(s);
                }
            }

        }

    }
}
