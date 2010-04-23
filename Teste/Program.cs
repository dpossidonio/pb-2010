﻿using System;
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
            while (true)
            {
                var c = Directory.GetCurrentDirectory();
                string[] CurrentDirectory = Regex.Split(c, "Teste");
                string ClientDirectory = CurrentDirectory[0] + "Client\\bin\\Debug\\Client.exe";
                string ServerDirectory = CurrentDirectory[0] + "Server\\bin\\Debug\\Server.exe";
                Read:
                Console.Write("Number of Clients: ");
                try{
                    
                    int numClientsInt = Convert.ToInt32(Console.ReadLine());
                    for (int a = 0; a < numClientsInt; a++)
                    {
                        var b = new Process();
                        b.StartInfo.FileName = ClientDirectory;
                        b.StartInfo.Arguments = "Program.cs";
                        b.Start();
                        for (int d = 0; d < 3; d++)
                        {
                            var e = new Process();
                            e.StartInfo.FileName = ServerDirectory;
                            e.StartInfo.Arguments = "8002";
                            e.Start();
                        }
                    }
                }
                catch(FormatException)
                {goto Read;}
            }
        }
    }
}