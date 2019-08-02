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
        int sessionID;
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
                string receivedMessage = receivePlain(); //Should be rand_cookie
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
                }


            }
        }

        public void send(string Message)
        {
            ClientSocket.Send(Encryption.Encrypt(Message, privateKeys[clientID]));
        }

        public void sendChat(string message)
        {
            TCPConnection clientB = cIDtoTCP[chattingWith];
            clientB.send(message);
            ChatHistory chat = new ChatHistory(clientID, clientB.clientID);
            chat.addLine(message);
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
                TCPConnection clientB = null;
                while (true)
                {
                    string clientMessage = receive();
                    string[] split = stringSplit(clientMessage);
                    if (chatting && split[0].Equals("CHAT"))
                    {
                        if (logOff(clientMessage))
                        {
                            //send log off message
                        }
                        else
                        {
                            sendChat(sessionID+" "+clientID + ": " + split[1]);
                            send(sessionID+" "+clientID + ": " + split[1]);
                        }
                    }
                    if (commandHistory(clientMessage) != null)
                    {
                        send(commandHistory(clientMessage));
                    }else if(commandChat(clientMessage)!=null)
                    {
                        clientB = commandChat(clientMessage);
                        send("CHAT_STARTED " + sessionID+" "+clientB.clientID);
                        clientB.send("CHAT_STARTED " +sessionID+" "+ clientID);
                    }
                    else if(!chatting)
                    {
                        send(clientMessage);
                    }

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                clientIDSocket.Remove(clientID);
                if(chatting)
                {
                    tearConnection(cIDtoTCP[chattingWith]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                clientIDSocket.Remove(clientID);
                if (chatting)
                {
                    tearConnection(cIDtoTCP[chattingWith]);
                }
            }
        }

        string commandHistory(string command)
        {
            string[] firstSpace = stringSplit(command);
            if (firstSpace[0].ToLower().Equals("history"))
            {
                Console.WriteLine("Requesting: " + clientID + " with clientB: \"" + firstSpace[1]+"\"");
                ChatHistory chatHistory = new ChatHistory(clientID, firstSpace[1]);
                string chats = chatHistory.chatHistory();
                /*chats = "CHAT " + chats;
                for(int i = 0; i < chats.Length; i++)
                {
                    if(chats[i]=='\n')
                    {
                        chats = chats.Substring(0, i) +"CHAT "+ chats.Substring(i);
                    }
                }*/
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
                        if(clientB.chattingWith==null || clientB.chattingWith.Equals(clientID) || clientB.chatting==false)
                        {
                            setupConnection(clientB);
                            return clientB;
                        }
                    }
                }
            }
            return null;
        }

        void setupConnection(TCPConnection clientB)
        {
            clientB.chattingWith = clientID;
            clientB.chatting = true;
            chattingWith = clientB.clientID;
            chatting = true;
            sessionID = challenge();
            clientB.sessionID = sessionID;
        }
        void tearConnection(TCPConnection clientB)
        {
            if(chatting)
            {
                clientB.chattingWith = "";
                clientB.chatting = false;
                chatting = false;
                chattingWith = "";
                sessionID = -1;
                clientB.sessionID = -1;
            }
        }
        int challenge()
        {
            Random rng = new Random();
            return rng.Next();
        }

        bool logOff(string command)
        {
            if(command.ToLower().Equals("end_request"))
            {
                send("END_NOTIF");
                sendChat("END_NOTIF");
                tearConnection(cIDtoTCP[chattingWith]);
                return true;
            }
            return false;
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
