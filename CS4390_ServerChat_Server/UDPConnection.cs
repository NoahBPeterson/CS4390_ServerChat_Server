using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text;

namespace CS4390_ServerChat_Server
{
    public class UDPConnection
    {
        Dictionary<string, byte[]> challengeAuthentication = new Dictionary<string, byte[]>();

        Socket sock = null;

        public UDPConnection()
        {

        }


        public int UDPReceive()
        {
            string receiveString = "";

            EndPoint hostEndPoint = new IPEndPoint(IPAddress.Any, 10020);
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                sock = new Socket(hostEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                sock.ReceiveTimeout=1200000;
                sock.Bind(hostEndPoint);
                byte[] receiveBytes = new byte[1024]; //1 KB

                while (true)
                {
                    Console.WriteLine("Waiting!"); //Debugging
                    Int32 receive = sock.ReceiveFrom(receiveBytes, ref clientEndPoint);
                    receiveString += Encoding.ASCII.GetString(receiveBytes);
                    receiveString = receiveString.Substring(0, receive);//Add this data to receiveString

                    if (clientID(receiveString))
                    {
                        int challengeResult = challenge();
                        byte[] challengeBuffer = challengeHash(challengeResult, receiveString);
                        challengeAuthentication[receiveString] = challengeBuffer; //Add "ID", challenge to hashmap for later use.
                        sock.SendTo(challengeBuffer, clientEndPoint);
                    }else //Change later. If response matches any of the valid authentication responses, respond with cookie and tcp port number
                    {
                        string clientID = "";
                        int responseStart = -1;
                        for(int i = 0; i < receiveString.Length; i++) //Client sent "[clientID] [challengeResponse]", separate them.
                        {
                            if(receiveString[i]!= ' ')
                            {
                                clientID += receiveString[i];
                            }
                            else
                            {
                                responseStart = i + 1;
                                break;
                            }
                        }
                        Console.WriteLine(clientID);
                        byte[] clientResponse = Encoding.ASCII.GetBytes(receiveString.Substring(responseStart, receiveString.Length - responseStart)); //Get challenge response, encode in byte[]
                        byte[] challengeA = challengeAuthentication[clientID]; //Get challenge from hashmap that the client should have independently created
                        if(clientResponse.Equals(challengeA))    //Authenticate. Send AUTH_SUCCESS(rand_cookie, tcp_port_number)
                        {
                            int rand_cookie = challenge();
                            sock.SendTo(Encoding.ASCII.GetBytes(rand_cookie+" "+10021), clientEndPoint);
                            Console.WriteLine(rand_cookie + " " + 10021);
                            return rand_cookie;
                        }
                        else     //Do not authenticate. Send AUTH_FAIL

                        {
                            sock.SendTo(Encoding.ASCII.GetBytes("FAILED"), clientEndPoint);

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

            return -1; //Return response from server.
        }

        public void UDPSend(IPEndPoint client, string message)
        {
            byte[] tcpPortNumber = Encoding.ASCII.GetBytes(message);
            sock.SendTo(tcpPortNumber, client);
        }

        int challenge()
        {
            Random rng = new Random();
            return rng.Next();
        }

        //This function checks the client ID to verify if it's valid.
        //Is it acceptable to hardcode IDs, or should we have a text file of a list of acceptable IDs?
        bool clientID(string clientID) 
        {
            switch(clientID)
            {
                case "noahb":
                    return true;
                default:
                    return true;
            }
        }

        string privateKey(string clientID) //Change this later. Text file, clientID : privateKey ?
        {
            return "password";
        }

        byte[] challengeHash(int challenge, string clientID)
        {
            SHA256 encryptionObject = SHA256.Create();
            byte[] hash = encryptionObject.ComputeHash(Encoding.ASCII.GetBytes(challenge.ToString() + privateKey(clientID)));
            return hash;
        }
    }
}