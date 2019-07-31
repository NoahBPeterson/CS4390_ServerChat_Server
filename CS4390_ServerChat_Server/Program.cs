using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace CS4390_ServerChat_Server
{
    class Program
    {
        static Dictionary<string, string> challengeAuthentication = new Dictionary<string, string>();
        static Dictionary<string, int> clientCookie = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            UDPConnection udpConnection = new UDPConnection(challengeAuthentication, clientCookie);
            TCPConnection tcpConnection = new TCPConnection(challengeAuthentication, clientCookie);
            Thread udp = new Thread(udpConnection.UDPReceive);
            Thread tcp = new Thread(tcpConnection.TCPConnect);
            udp.Start();
            tcp.Start();
            Console.ReadLine();
        }
    }
}
