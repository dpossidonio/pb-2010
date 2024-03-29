﻿using System;
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
            string address = "";
            int num_rep = 0;
            var rep_list = new List<string>();
            switch (args.Length)
            {
                case 0:
                    //paiva-alterei isso para fazer debug mais rapido é so apagar isso e descomentar o que está em baixo.
                    Console.Write("Enter [Port] to run: ");
                    address = "127.0.0.1:" + Console.ReadLine();
                    //Console.Write("Enter [IP:Port] to run: ");
                    //address = Console.ReadLine();
                    Console.Write("Enter number of Servers To Generate the Address:");
                    num_rep = int.Parse(Console.ReadLine());
                    Console.Write("or Enter address of one Server: 127.0.0.1:");
                    var adr = Console.ReadLine();
                    if (adr.Equals(""))
                        rep_list = GenerateReplicsAddress(address, num_rep);
                    else
                        rep_list = new List<string>(){ "127.0.0.1:" + adr};
                        break;
                case 2:
                    address = args[0];
                    num_rep = int.Parse(args[1]);
                    rep_list = GenerateReplicsAddress(address, num_rep);
                    break;
                case 3:
                    address = args[0];
                    num_rep = int.Parse(args[1]);
                    rep_list.Add(args[2]);
                    break;
                default:
                    Console.WriteLine("Error: Invalid arguments \nServer.exe [Address] [number_of_replics] [Replication_Server_Address]");
                    Console.ReadLine();
                    System.Environment.Exit(1);
                    break;
            }
            num_rep = num_rep % 10;

            Console.WriteLine("Welcome - PADIbook Server v1.0");
            Console.Title = "Server:" + address;

            //init
            ReplicaState = new StateContext(new SlaveState());
            State = new ServerState(address);
            sc = new ServerClient();
            State.ReplicationServers = rep_list;
            Server.sc.ServerServer.StartReplication();
            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("info"))
                    ReplicaState.RequestStateInfo();
                if (input.Split(' ')[0].Equals("freeze"))
                {
                    // Console.WriteLine("Valores em segundos");
                    //Console.Write("Freeze Period: ");
                    //  input = Console.ReadLine();
                    // Console.Write("Delay: ");
                    //  var input2 = Console.ReadLine();
                    try
                    {
                        State.Delay = int.Parse(input.Split(' ')[2]) * 1000;
                        State.FreezePeriod = int.Parse(input.Split(' ')[1]);
                        Console.Write("Freeze Period: {0}", State.FreezePeriod);
                        Console.WriteLine(" Delay: {0}", State.Delay);
                        State.FreezeTimeOver = DateTime.Now.AddSeconds(State.FreezePeriod);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("freeze [freeze_Period] [delay]");
                    }
                }

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

                if (input.Equals("ringdata"))
                    Console.WriteLine(sc.ServerServer.PrintInfoThatNodeIsResponsable());

                if (input.Equals("lookup"))
                {
                    Console.Write("Input name to search: ");
                    input = Console.ReadLine();
                    var s = sc.ServerServer.Lookup(sc.ServerServer.IDCreator(input));
                    Console.WriteLine(s);
                }
                if (input.Equals("finger"))
                    Console.WriteLine(sc.ServerServer.PrintFingerTable());
            }

        }

        //constroi uma lista com os end. das replicas servers
        public static List<string> GenerateReplicsAddress(string address, int num_rep)
        {
            var rep_list = new List<string>();
            for (int i = 1; i < num_rep + 1; i++)
            {
                var a = string.Format(address.Substring(0, address.Length - 1) + "{0}", i);
                rep_list.Add(a);
            }
            rep_list.Remove(address);
            return rep_list;
        }

    }
}
