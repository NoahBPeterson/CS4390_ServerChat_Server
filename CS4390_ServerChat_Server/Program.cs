using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CS4390_ServerChat_Server
{
    class Program
    {
        static Dictionary<string, int> clientCookie = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            UDPConnection udpConnection = new UDPConnection(clientCookie);
            Thread thread = new Thread(udpConnection.UDPReceive);
            thread.Start();
            thread.Join();
            Console.WriteLine("Program exited.");
            Console.ReadLine();
        }
    }
}
