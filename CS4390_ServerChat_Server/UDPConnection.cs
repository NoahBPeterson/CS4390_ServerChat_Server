using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CS4390_ServerChat_Server
{
    public class UDPConnection
    {
        public UDPConnection()
        {
        }


        public string UDPReceive()
        {
            Socket sock = null;
            int serverPort = 10020; //Receiving at port 10020
            string receiveString = "";

            try
            {
                IPEndPoint hostEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //SocketType.Dgram (UDP) /Stream (TCP)
                //ProtocolType.UDP/TCP

                string response = "";
                //Receive response from server using same socket.
                byte[] receiveBytes = null;
                while (true)
                {
                    Int32 receive = sock.Receive(receiveBytes); //Socket receives data into receiveBytes
                    receiveString += Encoding.ASCII.GetString(receiveBytes); //Add this data to receiveString

                    while (receive > 0)
                    {
                        Console.WriteLine("Received something!"); //Debugging
                        receive = sock.Receive(receiveBytes, receiveBytes.Length, 0);
                        receiveString += Encoding.ASCII.GetString(receiveBytes, receiveBytes.Length, 0);
                    }

                    if (receiveString == "noahb")
                    {
                        //CHALLENGE
                        int challengeResult = challenge();
                        byte[] challengeBuffer = BitConverter.GetBytes(challengeResult);
                        Console.WriteLine("Received HELLO from client! Sending challenge.");

                        sock.SendTo(challengeBuffer, hostEndPoint);
                    }
                    if (receiveString == "RESPONSE") //Change later. If response matches any of the valid authentication responses, respond with cookie and tcp port number
                    {
                        //Authenticate. Send AUTH_SUCCESS(rand_cookie, tcp_port_number)
                        //Do not authenticate. Send AUTH_FAIL
                    }

                    //Send "HELLO" to client using UDP.
                    string hello = "HELLO";
                    byte[] buffer = Encoding.ASCII.GetBytes(hello);
                    sock.SendTo(buffer, hostEndPoint);
                }


                sock.Close();  //Close socket when done.
            }
            catch (SocketException e) { }
            catch (ArgumentNullException e) { }
            catch (Exception e) { }

            return receiveString; //Return response from server.
        }

        int challenge()
        {
            Random rng = new Random();
            return rng.Next();
        }
    }
}