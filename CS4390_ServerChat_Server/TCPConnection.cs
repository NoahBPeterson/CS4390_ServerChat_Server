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
        Dictionary<string, string> privateKeys;
        Dictionary<int, string> clientCookies;
        Dictionary<string, Socket> clientIDSocket;
        IPEndPoint serverEndpoint;
        Socket ClientSocket;
        Socket ServerListener;
        string clientID;
        public TCPConnection(Dictionary<string, string> privateKeys, Dictionary<int, string> cookies, Dictionary<string, Socket> clientIDSocket)
        {
            this.privateKeys = privateKeys;
            clientCookies = cookies;
            serverEndpoint = new IPEndPoint(IPAddress.Any, 10021);
            this.clientIDSocket = clientIDSocket;
        }
        public TCPConnection(Dictionary<string, string> privateKeys, Dictionary<int, string> cookies, Socket clientSocket, Dictionary<string, Socket> clientIDSocket, string cID) //Used only for threading
        {
            this.privateKeys = privateKeys;
            clientCookies = cookies;
            serverEndpoint = new IPEndPoint(IPAddress.Any, 10021);
            ClientSocket = clientSocket;
            this.clientIDSocket = clientIDSocket;
            clientID = cID;
        }

        public void TCPConnect()
        {
            ServerListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerListener.Bind(serverEndpoint);
            ServerListener.Listen(100);
            Console.WriteLine("TCP Server is Listening...");
            Socket clientSocket = default(Socket);
            int counter = 0;
            Thread userThread;

            while (true)
            {
                counter++;
                clientSocket = ServerListener.Accept();
                ClientSocket = clientSocket;
                Console.WriteLine("{0} Clients connected!", counter);
                byte[] msgs = new byte[1024];
                string receivedMessage = receivePlain(); //Should be rand_cookie
                string clientID;
                //Verify that receivedMessage is a valid client cookie
                bool validCookie = clientCookies.TryGetValue(Int32.Parse(receivedMessage), out clientID);//clientCookies.ContainsValue(Int32.Parse(receivedMessage));
                clientIDSocket[clientID] = ClientSocket;

                if (validCookie)
                {
                    //Make this global so we can remove user threads as people timeout?
                    TCPConnection user = new TCPConnection(privateKeys, clientCookies, ClientSocket, clientIDSocket,  clientID);
                    //Thread UserThreads = new Thread(new ThreadStart(() => User(clientSocket)));
                    userThread = new Thread(user.User);
                    userThread.Start();
                }


            }
        }

        public void send(string Message)
        {
            ClientSocket.Send(Encryption.Encrypt(Message, privateKeys[clientID]));
        }

        public string receive()
        {
            byte[] msgFromServer = new byte[1024];
            int size = ClientSocket.Receive(msgFromServer);
            byte[] msg = new byte[size];
            Array.Copy(msgFromServer, msg, size);
            return Encryption.Decrypt(msg, privateKeys[clientID]);
        }

        public string receivePlain() {
            byte[] msgFromServer = new byte[1024];
            int size = ClientSocket.Receive(msgFromServer);
            return Encoding.UTF8.GetString(msgFromServer, 0, size);
        }

        public void User()//(Socket Client)
        {
            send("CONNECTED");
            try
            {
                while (true)
                {
                    Socket client = ClientSocket;
                    string message = receive();
                    send(message);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                clientIDSocket.Remove(clientID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                clientIDSocket.Remove(clientID);
            }
        }
    }
}
