using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Client
{

    public class Hello : System.MarshalByRefObject
    {

        public Hello()
        {
            Console.WriteLine("Constructor called");
        }

        ~Hello()
        {
            Console.WriteLine("Destructor called");
        }

        public string Greeting(string name)
        {
            Console.WriteLine("Greeting called");
            return ("Hello, " + name);
        }
    }

    //Which is used in a server with the following code:

    class HelloServer
    {
        static void Main2(string[] args)
        {
            // create the server side object,
            // which I want to use remotely.

            Hello h = new Hello();
            TcpServerChannel channel = new TcpServerChannel(8086);
            ChannelServices.RegisterChannel(channel);
            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(Hello),
            "Hi",
            WellKnownObjectMode.Singleton);
            System.Console.WriteLine("hit enter to exit");
            System.Console.ReadLine();

        }

    }

    //The client running on another machine has the following code:

    class HelloClient
    {
        void Main2()
        {
            ChannelServices.RegisterChannel(new TcpClientChannel());
            Hello obj = (Hello)Activator.GetObject(
            typeof(Hello),
            "tcp://kzin:8086/Hi");
            if (obj == null)
            {
                Console.WriteLine("could not locate server");
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(obj.Greeting("Bill"));
            }
        }
    }
}

