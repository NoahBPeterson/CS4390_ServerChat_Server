using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS4390_ServerChat_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            UDPConnection udpConnection = new UDPConnection();
            while(true)
            {
                udpConnection.UDPReceive(); //Assign this its own thread so it doesn't tie up the execution of the whole program.
            }
            Console.WriteLine("Program exited.");
        }
    }
}
