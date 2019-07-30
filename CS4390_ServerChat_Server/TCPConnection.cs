using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS4390_ServerChat_Server
{
    class TCPConnection
    {
        Dictionary<string, int> clientCookies;
        IPEndPoint serverEndpoint;
        Socket ClientSocket;
        public TCPConnection(IPEndPoint server, Dictionary<string, int> cookies)
        {
            clientCookies = cookies;
            serverEndpoint = server;
        }

        public void TCPConnect()
        {
            Socket ServerListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerListener.Bind(serverEndpoint);
            ServerListener.Listen(100);
            Console.WriteLine("Server is Listening...");
            Socket clientSocket = default(Socket);
            int counter = 0;
            while (true)
            {
                counter++;
                clientSocket = ServerListener.Accept();
                Console.WriteLine("{0} Clients connected!", counter);
                byte[] msgs = new byte[1024];
                int msgSize = clientSocket.Receive(msgs);
                string receivedMessage = Encoding.ASCII.GetString(msgs); //Should be rand_cookie
                //Verify that receivedMessage is a valid client cookie

                //Make this global so we can remove user threads as people timeout?
                Thread UserThreads = new Thread(new ThreadStart(() => User(clientSocket)));

            }
        }

        public void send(string Message)
        {
            ClientSocket.Send(System.Text.Encoding.ASCII.GetBytes(Message), 0, Message.Length, SocketFlags.None);

        }

        public string receive()
        {
            byte[] msgFromServer = new byte[1024];
            int size = ClientSocket.Receive(msgFromServer);
            return System.Text.Encoding.ASCII.GetString(msgFromServer, 0, size));
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
