using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text;

namespace CS4390_ServerChat_Server
{
    public class UDPConnection
    {
        private AsyncCallback recv = null;
        private State state = new State();

        Socket sock = null;

        public class State
        {
            public byte[] buffer = new byte[8192];
        }
        public UDPConnection()
        {

        }


        public EndPoint UDPReceive()
        {
            int serverPort = 10020; //Receiving at port 10020
            string receiveString = "";

            EndPoint hostEndPoint = new IPEndPoint(IPAddress.Any, 10020);
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                sock = new Socket(hostEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                sock.ReceiveTimeout=1200000;
                sock.Bind(hostEndPoint);
                byte[] receiveBytes = new byte[1024]; //1 KB

                string response = "";
                //Receive response from server using same socket.
                while (true)
                {
                    Console.WriteLine("Waiting!"); //Debugging
                    Int32 receive = sock.ReceiveFrom(receiveBytes, ref clientEndPoint);
                    receiveString += Encoding.ASCII.GetString(receiveBytes);
                    receiveString = receiveString.Substring(0, receive);//Add this data to receiveString
                    //Console.WriteLine("Received something: "+ receiveString); //Debugging

                    //Client IP:Port debugging: Making sure we are getting client IP:Port
                    /*int port = ((IPEndPoint)clientEndPoint).Port;
                    string ipAddr = ((IPEndPoint)clientEndPoint).Address.ToString();
                    Console.WriteLine("Client ip:port "+ipAddr+":" + port);*/

        
                    /*while (receive > 0)
                    {
                        Console.WriteLine("Received something: "+receiveString); //Debugging
                        receive = sock.ReceiveFrom(receiveBytes, receiveBytes.Length, 0, ref hostEndPoint);
                        receiveString += Encoding.ASCII.GetString(receiveBytes, receiveBytes.Length, 0);
                    }*/
                    if (clientID(receiveString)
                    {
                        return clientEndPoint;

                        //CHALLENGE- TODO later, when doing security.
                        /*int challengeResult = challenge();
                        byte[] challengeBuffer = BitConverter.GetBytes(challengeResult);
                        sock.SendTo(challengeBuffer, clientEndPoint);*/
                    }
                    if (receiveString == "RESPONSE") //Change later. If response matches any of the valid authentication responses, respond with cookie and tcp port number
                    {
                        //Authenticate. Send AUTH_SUCCESS(rand_cookie, tcp_port_number)
                        //Do not authenticate. Send AUTH_FAIL
                    }

                    //Send "HELLO" to client using UDP.
                    string hello = "HELLO";
                    byte[] buffer = Encoding.ASCII.GetBytes(hello);
                    sock.SendTo(buffer, clientEndPoint);
                    receiveBytes = new byte[1024];
                    receiveString = "";
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

            return clientEndPoint; //Return response from server.
        }

        public string UDPSend(IPEndPoint client, int tcpPort)
        {
            byte[] tcpPortNumber = BitConverter.GetBytes(tcpPort);
            sock.SendTo(tcpPortNumber, client);
            return "";
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

        bool challengeAccept(string challenge)
        {
            return true;
        }
    }
}