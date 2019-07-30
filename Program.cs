using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 1000;
            string IpAdress = "127.0.0.1";
            Socket ServerListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpAdress), port);
            ServerListener.Bind(ep);
            ServerListener.Listen(100);
            Console.WriteLine("Server is Listening...");
            Socket clientSocket = default(Socket);
            Program p = new Program();
            int counter = 0;
            while (true)
            {
                counter++;
                clientSocket = ServerListener.Accept();
                Console.WriteLine("{0} Clients connected!", counter);
                Socket client;
                byte[] msgs = new byte[1024];
                int size = clientSocket.Receive(msgs);
                Console.WriteLine(Encoding.ASCII.GetString(msgs));

                Thread UserThreads = new Thread(new ThreadStart(() => p.User(clientSocket)));

            }

        }
        public void User(Socket client)
        {
            while (true)
            {
                byte[] msgs = new byte[1024];
                int size = client.Receive(msgs);
                client.Send(msgs, 0, size, SocketFlags.None);
            }
        }

    }
}
