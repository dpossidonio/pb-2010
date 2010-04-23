using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Client
{
    static class ProgramClient
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string client_address="";
            var list_server_address = new List<string>();
            if (args.Length > 0) {
                client_address = args[0];
                for (int i = 1; i < args.Length; i++)
                {
                    list_server_address.Add(args[i]);
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UIClient(client_address,list_server_address));
        }
    }
}
