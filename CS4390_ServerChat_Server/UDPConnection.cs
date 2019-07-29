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
        Dictionary<string, int> clientRandomCookies;
        Socket sock = null;

        public UDPConnection(Dictionary<string, int> clientCookies)
        {
            clientRandomCookies = clientCookies;
        }


        public int UDPReceive()
        {

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
                    string receiveString = "";
                    Console.WriteLine("Waiting!"); //Debugging
                    Int32 receive = sock.ReceiveFrom(receiveBytes, ref clientEndPoint);
                    receiveString += Encoding.ASCII.GetString(receiveBytes);
                    receiveString = receiveString.Substring(0, receive);//Add this data to receiveString

                    string Hello = "";
                    if (receiveString.Length == 5 && receiveString.Equals("HELLO"))
                    {
                        Hello = receiveString;
                    }

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
                                responseStart = i;
                                break;
                            }
                        }
                        string clientChallengeResponse = receiveString.Substring(responseStart + 1, receiveString.Length - (responseStart +1));
                        byte[] clientResponse = Encoding.ASCII.GetBytes(clientChallengeResponse); //Get challenge response, encode in byte[]
                        byte[] challengeA = challengeAuthentication[clientID]; //Get challenge from hashmap that the client should have independently created
                        string ourChallenge = Encoding.ASCII.GetString(challengeA);
                        if(clientChallengeResponse.Equals(ourChallenge))    //Authenticate. Send AUTH_SUCCESS(rand_cookie, tcp_port_number)
                        {
                            int rand_cookie = challenge();
                            sock.SendTo(Encoding.ASCII.GetBytes(rand_cookie+" "+10021), clientEndPoint);
                            Console.WriteLine("Rand_cookie + \" \" + 10021:"+rand_cookie + " " + 10021);
                            clientRandomCookies[clientID] = rand_cookie; //Rand_Cookie now added to dictionary accessible from driver function.
                            //return rand_cookie;
                        }
                        else     //Do not authenticate. Send AUTH_FAIL
                        {
                            Console.WriteLine("FAIL! Client authentication: "+clientChallengeResponse+" Our authentication: "+ourChallenge);
                            sock.SendTo(Encoding.ASCII.GetBytes("FAIL"), clientEndPoint);

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
                    return false;
            }
        }

        string privateKey(string clientID) //Change this later. Text file, clientID : privateKey ?
        {
            return "password";
        }

        byte[] challengeHash(int challenge, string clientID)
        {
            SHA256 encryptionObject = SHA256.Create();
            string challengeToString = challenge.ToString();
            byte[] challengeBytes = Encoding.ASCII.GetBytes(challengeToString);
            byte[] hash = encryptionObject.ComputeHash(Encoding.ASCII.GetBytes(challengeBytes+privateKey(clientID)));
            return hash;
        }
    }
}