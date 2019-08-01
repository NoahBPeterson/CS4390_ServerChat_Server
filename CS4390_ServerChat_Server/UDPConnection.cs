using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text;

namespace CS4390_ServerChat_Server
{
    public class UDPConnection
    {
        Dictionary<string, string> challengeAuthentication;
        Dictionary<int, string> clientRandomCookies;
        Socket sock = null;

        public UDPConnection(Dictionary<string, string> cipherKeys, Dictionary<int, string> clientCookies)
        {
            challengeAuthentication = cipherKeys;
            clientRandomCookies = clientCookies;
        }


        public void UDPReceive()
        {

            EndPoint hostEndPoint = new IPEndPoint(IPAddress.Any, 10020);
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {

                sock = new Socket(hostEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                sock.ReceiveTimeout=1200000;
                sock.Bind(hostEndPoint);
                byte[] receiveBytes = new byte[1024]; //1 KB
                Console.WriteLine("UDP Server is Listening...");

                while (true)
                {
                    string receiveString = "";
                    Int32 receive = sock.ReceiveFrom(receiveBytes, ref clientEndPoint);
                    receiveString += Encoding.UTF8.GetString(receiveBytes);
                    receiveString = receiveString.Substring(0, receive);//Add this data to receiveString

                    string cID = "";
                    int responseStart = -1;
                    for (int i = 0; i < receiveString.Length; i++) //Client sent "[clientID] [challengeResponse]", separate them.
                    {
                        if (receiveString[i] != ' ')
                        {
                            cID += receiveString[i];
                        }
                        else
                        {
                            responseStart = i;
                            break;
                        }
                    }

                    if (clientID(receiveString))
                    {

                        int challengeResult = challenge();
                        string cipher = challengeResult.ToString() + privateKey(receiveString);
                        byte[] challengeBuffer = challengeHash(cipher);
                        string challengeString = Encoding.UTF8.GetString(challengeBuffer);
                        challengeAuthentication[receiveString] = cipher; //Add "ID", challenge to hashmap for later use.
                        byte[] challengeResultBytes = Encoding.UTF8.GetBytes(challengeResult.ToString());
                        sock.SendTo(challengeResultBytes, clientEndPoint); //Send random integer challenge, encoded in UTF8
                    }
                    else if(clientID(cID)) //Change later. If response matches any of the valid authentication responses, respond with cookie and tcp port number
                    {
                        string serverChallenge;
                        challengeAuthentication.TryGetValue(cID, out serverChallenge); //Get challenge from hashmap that the client should have independently created
                        byte[] serverChallengeByte = challengeHash(serverChallenge);
                        string serverChallengeString = Encoding.UTF8.GetString(serverChallengeByte);

                        string clientResponseString = receiveString.Substring(responseStart + 1, receiveString.Length - (responseStart+1)).TrimEnd(new char[] { (char)0 });



                        if (serverChallengeString.Equals(clientResponseString))    //Authenticate. Send AUTH_SUCCESS(rand_cookie, tcp_port_number)
                        {
                            int rand_cookie = challenge();
                            string cookie_port = Encrypt(rand_cookie.ToString() + " " + 10021, serverChallenge); //cipher: rand+password, '+' is concatenation.
                            sock.SendTo(Encoding.UTF8.GetBytes(cookie_port), clientEndPoint);
                            Console.WriteLine("Rand_cookie + \" \" + 10021:"+rand_cookie + " " + 10021);
                            clientRandomCookies[rand_cookie] = cID; //Rand_Cookie now added to dictionary accessible from driver function.
                            //return rand_cookie;
                        }
                        else     //Do not authenticate. Send AUTH_FAIL
                        {
                            Console.WriteLine("FAIL! Client authentication: \"" + clientResponseString + "\" \n Our authentication: \"" + serverChallengeString);
                            sock.SendTo(Encoding.UTF8.GetBytes("FAIL"), clientEndPoint);

                        }
                    }
                }


                sock.Close();  //Close socket when done.
            }
            catch (SocketException e) {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentNullException e) {
                Console.WriteLine(e.Message);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public void UDPSend(IPEndPoint client, string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            sock.SendTo(msg, client);
        }

        int challenge()
        {
            Random rng = new Random();
            return rng.Next();
        }

        bool clientID(string clientID) 
        {
            StreamReader streamReader = new StreamReader("users.txt");
            string line;
            string[] split;
            do
            {
                line = streamReader.ReadLine();
                split = line.Split(' ');

                if (split[0].Equals(clientID)) return true;
            } while (split[0] != clientID && !streamReader.EndOfStream);
            streamReader.Close();
            return false;
        }

        string privateKey(string clientID)
        {
            StreamReader streamReader = new StreamReader("users.txt");
            string line = streamReader.ReadLine();
            string[] split = line.Split(' ');
            while(!split[0].Equals(clientID))
            {
                line = streamReader.ReadLine();
                split = line.Split(' ');
            }
            streamReader.Close();
            return split[1];
        }

        byte[] challengeHash(string cipher)
        {
            SHA256 encryptionObject = SHA256.Create();
            byte[] hash = encryptionObject.ComputeHash(Encoding.UTF8.GetBytes(cipher));
            string hashString = Encoding.UTF8.GetString(hash);
            return hash;
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