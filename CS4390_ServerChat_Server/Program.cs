using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CS4390_ServerChat_Server
{
    class Program
    {
        static Dictionary<string, int> clientCookie = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            UDPConnection udpConnection = new UDPConnection(clientCookie);
            int cookie = udpConnection.UDPReceive(); //Assign this its own thread so it doesn't tie up the execution of the whole program.
            Console.WriteLine("Program exited."+cookie);
            Console.ReadLine();
        }
    }
}
