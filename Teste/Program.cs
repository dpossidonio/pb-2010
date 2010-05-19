using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace Teste
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to PADIbook Test Console");
            int MAX_SERVERS = 5;
            var c = Directory.GetCurrentDirectory();
            string[] CurrentDirectory = Regex.Split(c, "Teste");
            string ClientDirectory = CurrentDirectory[0] + "Client\\bin\\Debug\\Client.exe";
            string ServerDirectory = CurrentDirectory[0] + "Server\\bin\\Debug\\Server.exe";
           // string ClientDirectory = "Client.exe";
            //string ServerDirectory = "Server.exe";
            while (true)
            {
                string client_address;
                int num_replic;
                while (true)
                {
                    Console.Write("Client's Address: 127.0.0.1:");
                    client_address = "127.0.0.1:";
                    client_address += Console.ReadLine();

                    //var client_address = Console.ReadLine();
                    Console.Write("Number of Servers: ");
                    num_replic = int.Parse(Console.ReadLine());
                    var a = client_address[client_address.Length - 1];
                    if (client_address[client_address.Length - 1].Equals('0') && num_replic <= MAX_SERVERS)
                        break;
                    else Console.WriteLine("#Error:Address must end with '0' and number of Servers must be lower than {0}.",MAX_SERVERS);
                }
                try
                {
                    var port_client = int.Parse(client_address.Split(':')[1]);
                    var b = new Process();
                    b.StartInfo.FileName = ClientDirectory;
                    var arg = client_address;
                    for (int i = 1; i < num_replic + 1; i++)
                    {
                        arg += string.Format(" 127.0.0.1:{0}", port_client + i);
                    }
                        b.StartInfo.Arguments = arg;
                    b.Start();
                    for (int d = 1; d < num_replic+1; d++)
                    {
                        var e = new Process();
                        e.StartInfo.FileName = ServerDirectory;
                        e.StartInfo.Arguments = string.Format("127.0.0.1:{0} {1}", port_client + d,num_replic);
                        e.Start();
                    }
                }
                catch (Exception) { Console.WriteLine("Error: Invalid Address"); }
            }
        }
    }
}
