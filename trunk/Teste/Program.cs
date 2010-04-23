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
            var c = Directory.GetCurrentDirectory();
            string[] CurrentDirectory = Regex.Split(c, "Teste");
            string ClientDirectory = CurrentDirectory[0] + "Client\\bin\\Debug\\Client.exe";
            string ServerDirectory = CurrentDirectory[0] + "Server\\bin\\Debug\\Server.exe";
            while (true)
            {
                Console.Write("Client's Adress: ");
                var client_address = Console.ReadLine();
                var port_client = int.Parse(client_address.Split(':')[1]);
                var b = new Process();
                b.StartInfo.FileName = ClientDirectory;
                b.StartInfo.Arguments = string.Format("{0} 127.0.0.1:{1} 127.0.0.1:{2} 127.0.0.1:{3}", client_address, port_client + 1, port_client + 2, port_client + 3);
                b.Start();
                for (int d = 1; d < 3; d++)
                {
                    var e = new Process();
                    e.StartInfo.FileName = ServerDirectory;
                    e.StartInfo.Arguments = string.Format("127.0.0.1:{0} 3", port_client + d);
                    e.Start();
                }

            }
        }
    }
}
