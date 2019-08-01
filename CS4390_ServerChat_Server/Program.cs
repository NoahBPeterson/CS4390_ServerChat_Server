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
        Dictionary<string, string> challengeAuthentication = new Dictionary<string, string>();
        Dictionary<int, string> clientCookie = new Dictionary<int, string>();

        static void Main(string[] args)
        {
            Program nonStatic = new Program();
            UDPConnection udpConnection = new UDPConnection(nonStatic.challengeAuthentication, nonStatic.clientCookie);
            TCPConnection tcpConnection = new TCPConnection(nonStatic.challengeAuthentication, nonStatic.clientCookie);
            Thread udp = new Thread(udpConnection.UDPReceive);
            Thread tcp = new Thread(tcpConnection.TCPConnect);
            udp.Start();
            tcp.Start();
            Console.ReadLine();
        }
    }
}
