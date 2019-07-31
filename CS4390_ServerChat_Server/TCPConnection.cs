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
                string receivedMessage = Encoding.UTF8.GetString(msgs); //Should be rand_cookie
                //Verify that receivedMessage is a valid client cookie
                bool validCookie = clientCookies.ContainsValue(Int32.Parse(receivedMessage));

                if (validCookie)
                {
                    //Make this global so we can remove user threads as people timeout?
                    Thread UserThreads = new Thread(new ThreadStart(() => User(clientSocket)));
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

        public void User(Socket client)
        {
            while (true)
            {
                byte[] msgs = new byte[1024];
                int size = client.Receive(msgs);
                client.Send(msgs, 0, size, SocketFlags.None);
            }
        }

        public static string Encrypt(string messageSent, string Cipher)
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

        public static string Decrypt(string encryptedMessage, string Cipher)
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
