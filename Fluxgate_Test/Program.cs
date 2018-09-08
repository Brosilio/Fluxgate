using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Fluxgate;

namespace Fluxgate_Test
{
    class Program
    {
        static Listener c = new Listener();
        static Client cli = new Client();

        static Client[] clients = new Client[256];

        static void Main(string[] args)
        {
            c.onNewTransmitter += onConnect;
            c.Listen(6832, 1);
            Thread t = new Thread(DataLoop);
            t.Start();

            cli.Connect("localhost", 6832);

            while(true)
            {
                cli.Write(Encoding.ASCII.GetBytes(Console.ReadLine()));
            }
        }

        private static void onConnect(Client obj)
        {
            for(int i = 0; i < clients.Length; i++)
            {
                if (clients[i] == null)
                {
                    clients[i] = obj;
                    return;
                }
            }
        }

        public static void DataLoop()
        {
            while(true)
            {
                for (int i = 0; i < clients.Length; i++)
                {
                    if (clients[i] != null && clients[i].IsConnected)
                    {
                        if (clients[i].IsDataAvailable())
                        {
                            Console.WriteLine("Data available: " + clients[i].AmountDataAvailable());
                        }else
                        {
                            Console.WriteLine("No Data available: " + clients[i].AmountDataAvailable());
                        }
                    }
                }
            }
        }
    }
}