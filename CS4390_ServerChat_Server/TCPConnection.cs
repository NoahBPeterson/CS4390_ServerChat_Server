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
        static Dictionary<string, TCPConnection> cIDtoTCP;
        IPEndPoint serverEndpoint;
        Socket ClientSocket;
        Socket ServerListener;
        string clientID;
        bool chatting = false;
        string chattingWith;
        public TCPConnection(Dictionary<string, string> privateKeys, Dictionary<int, string> cookies, Dictionary<string, Socket> clientIDSocket)
        {
            this.privateKeys = privateKeys;
            clientCookies = cookies;
            serverEndpoint = new IPEndPoint(IPAddress.Any, 10021);
            this.clientIDSocket = clientIDSocket;
            cIDtoTCP = new Dictionary<string, TCPConnection>();
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
                int msgSize = clientSocket.Receive(msgs);
                string receivedMessage = Encoding.UTF8.GetString(msgs); //Should be rand_cookie
                string clientID;
                //Verify that receivedMessage is a valid client cookie
                bool validCookie = clientCookies.TryGetValue(Int32.Parse(receivedMessage), out clientID);//clientCookies.ContainsValue(Int32.Parse(receivedMessage));
                clientIDSocket[clientID] = ClientSocket;

                if (validCookie)
                {
                    //Make this global so we can remove user threads as people timeout?
                    TCPConnection user = new TCPConnection(privateKeys, clientCookies, ClientSocket, clientIDSocket,  clientID);
                    cIDtoTCP[clientID] = user;
                    //Thread UserThreads = new Thread(new ThreadStart(() => User(clientSocket)));
                    userThread = new Thread(user.User);
                    userThread.Start();
                    send(Encryption.Encrypt("CONNECTED", privateKeys[clientID]));
                }


            }
        }

        public void send(string Message)
        {

            ClientSocket.Send(System.Text.Encoding.UTF8.GetBytes(Message), 0, Message.Length, SocketFlags.None);

        }

        public void send(byte[] data) {
            ClientSocket.Send(data);
        }

        public string receive()
        {
            byte[] msgFromServer = new byte[1024];
            int size = ClientSocket.Receive(msgFromServer);
            return System.Text.Encoding.UTF8.GetString(msgFromServer, 0, size);
        }



        public void User()//(Socket Client)
        {
            try
            {
                TCPConnection clientB = null;
                while (true)
                {
                    Socket client = ClientSocket;
                    byte[] msgs = new byte[1024];
                    int size = client.Receive(msgs);
                    string clientMessage = Encoding.UTF8.GetString(msgs).TrimEnd(new char[] { (char)0 });
                    if(commandHistory(clientMessage) != null)
                    {
                        byte[] ciphered = Encryption.Encrypt(commandHistory(clientMessage), privateKeys[clientID]);
                        client.Send(ciphered, 0, ciphered.Length, SocketFlags.None);
                    }else if(commandChat(clientMessage)!=null)
                    {
                        clientB = commandChat(clientMessage);
                        //Do something if "Chat (client-id-b) is sent."
                    }
                    else
                    {
                        byte[] cipherBytes = Encryption.Encrypt(clientMessage, privateKeys[clientID]);
                        client.Send(cipherBytes, 0, cipherBytes.Length, SocketFlags.None);
                    }
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

        string commandHistory(string command)
        {
            string[] firstSpace = stringSplit(command);
            if (firstSpace[0].ToLower().Equals("history"))
            {
                ChatHistory chatHistory = new ChatHistory(clientID, firstSpace[1]);
                string chats = chatHistory.chatHistory();
                if(chats!=null)
                {
                    return chats;
                }
            }
            return null;
        }

        TCPConnection commandChat(string command)
        {
            string[] firstSpace = stringSplit(command);
            if(firstSpace[0].ToLower().Equals("chat"))
            {
                if(firstSpace.Length!=2) //chat client-id-b. Nothing else should work.
                {
                    return null;
                }
                if (clientIDSocket.ContainsKey(firstSpace[1]) && clientID != firstSpace[1])
                {
                    TCPConnection clientB;
                    cIDtoTCP.TryGetValue(firstSpace[1], out clientB);
                    if (clientB != null)
                    {
                        if(clientB.chattingWith==null || clientB.chattingWith.Equals(clientID))
                        {
                            return clientB;
                        }
                    }
                }
            }
            return null;
        }

        string[] stringSplit(string line)
        {
            string cookie = "";
            string port = "";
            int space = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    space = i;
                    break;
                }
                else
                {
                    cookie += line.Substring(i, 1);
                }
            }
            port = line.Substring(space, line.Length - space);
            cookie = cookie.Trim();
            port = port.Trim();
            string[] cookiePort = { cookie, port };
            return cookiePort;
        }
    }
}
