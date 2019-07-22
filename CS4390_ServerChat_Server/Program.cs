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
        static void Main(string[] args)
        {
            UDPConnection udpConnection = new UDPConnection();
            int cookie = udpConnection.UDPReceive(); //Assign this its own thread so it doesn't tie up the execution of the whole program.
            //client = EndPoint
            Console.WriteLine("Program exited."+cookie);
            Console.ReadLine();
        }
    }
}
