using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS4390_ServerChat_Server
{
    class TCPConnection
    {
        Dictionary<string, string> privateKeys;
        Dictionary<string, int> clientCookies;
        IPEndPoint serverEndpoint;
        Socket ClientSocket;
        Socket ServerListener;
        public TCPConnection(Dictionary<string, string> privateKeys, Dictionary<string, int> cookies)
        {
            this.privateKeys = privateKeys;
            clientCookies = cookies;
            serverEndpoint = new IPEndPoint(IPAddress.Any, 10021);
            
        }
        public TCPConnection(Dictionary<string, string> privateKeys, Dictionary<string, int> cookies, Socket clientSocket) //Used only for threading
        {
            this.privateKeys = privateKeys;
            clientCookies = cookies;
            serverEndpoint = new IPEndPoint(IPAddress.Any, 10021);
            ClientSocket = clientSocket;
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
                //Verify that receivedMessage is a valid client cookie
                bool validCookie = clientCookies.ContainsValue(Int32.Parse(receivedMessage));

                if (validCookie)
                {
                    //Make this global so we can remove user threads as people timeout?
                    TCPConnection user = new TCPConnection(privateKeys, clientCookies, ClientSocket);
                    //Thread UserThreads = new Thread(new ThreadStart(() => User(clientSocket)));
                    userThread = new Thread(user.User);
                    userThread.Start();
                    send("CONNECTED");
                }


            }
        }

        public void send(string Message)
        {

            ClientSocket.Send(System.Text.Encoding.UTF8.GetBytes(Message), 0, Message.Length, SocketFlags.None);

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
                while (true)
                {
                    Socket client = ClientSocket;
                    byte[] msgs = new byte[1024];
                    int size = client.Receive(msgs);
                    string clientMessage = Encoding.UTF8.GetString(msgs).TrimEnd(new char[] { (char)0 });
                    client.Send(System.Text.Encoding.UTF8.GetBytes(clientMessage), 0, clientMessage.Length, SocketFlags.None);
                    //client.Send(msgs, 0, size, SocketFlags.None);   
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public string Encrypt(string messageSent, string Cipher)
        {
            using (var CryptoMD5 = new MD5CryptoServiceProvider())
            {
                using (var TripleDES = new TripleDESCryptoServiceProvider())
                {
                    TripleDES.Key = CryptoMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(Cipher));
                    TripleDES.Mode = CipherMode.ECB;
                    TripleDES.Padding = PaddingMode.PKCS7;

                    using (var crypt = TripleDES.CreateEncryptor())
                    {
                        byte[] messageBytes = UTF8Encoding.UTF8.GetBytes(messageSent);
                        byte[] totalBytes = crypt.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                        return Convert.ToBase64String(totalBytes, 0, totalBytes.Length);
                    }
                }
            }
        }

        public string Decrypt(string encryptedMessage, string Cipher)
        {
            using (var CryptoMD5 = new MD5CryptoServiceProvider())
            {
                using (var TripleDES = new TripleDESCryptoServiceProvider())
                {
                    TripleDES.Key = CryptoMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(Cipher));
                    TripleDES.Mode = CipherMode.ECB;
                    TripleDES.Padding = PaddingMode.PKCS7;

                    using (var crypt = TripleDES.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(encryptedMessage);
                        byte[] totalBytes = crypt.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return UTF8Encoding.UTF8.GetString(totalBytes);
                    }
                }
            }
        }
    }


}
