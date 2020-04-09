using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace CS3500
{

    class SocketState
    {
        public Socket theSocket;
        public byte[] messageBuffer;
        public StringBuilder sb;

        public SocketState(Socket s)
        {
            theSocket = s;
            messageBuffer = new byte[1024];
            sb = new StringBuilder();
        }
    }

    public class ChatClient
    {
        private static int port = -1;

        /// <summary>
        /// Entry point to chat client we ported from standalone application
        /// </summary>
        /// <param name="serverAddr">Chat server address</param>
        /// <param name="portNumber">Chat server port</param>
        public void StartClient(string serverAddr, string portNumber)
        {
            if (!Int32.TryParse(portNumber, out port))
            {
                throw new ArgumentException("Cannot parse port... exiting");
            }
            else
            {
                try
                {
                    ConnectToServer(serverAddr);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Error in connection {e}");
                }
            }
        }

        /// <summary>
        /// Starts the connection process
        /// </summary>
        /// <param name="serverAddr"></param>
        private void ConnectToServer(string serverAddr)
        {
            // Parse the IP
            IPAddress addr = IPAddress.Parse(serverAddr);

            // Create a socket
            Socket s1 = new Socket(addr.AddressFamily, SocketType.Stream,
              ProtocolType.Tcp);
            //// Create a socket
            //Socket s2 = new Socket(addr.AddressFamily, SocketType.Stream,
            //  ProtocolType.Tcp);

            SocketState ss1 = new SocketState(s1);
            //SocketState ss2 = new SocketState(s2);

            // Connect
            ss1.theSocket.BeginConnect(addr, port, OnConnected, ss1);
            //ss2.theSocket.BeginConnect(addr, port, OnConnected, ss2);
        }

        /// <summary>
        /// Callback for when a connection is made (see line 62)
        /// Finalizes the connection, then starts a receive loop.
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnected(IAsyncResult ar)
        {
            Console.WriteLine("Was able to contact the server and establish a connection");

            SocketState theServer = (SocketState)ar.AsyncState;

            // this does not end the connection! this simply acknowledges that we are at the _end_ of the start
            // of the connection phase!
            theServer.theSocket.EndConnect(ar);

            // Start a receive operation
            theServer.theSocket.BeginReceive(theServer.messageBuffer, 0, theServer.messageBuffer.Length, SocketFlags.None, 
                OnReceive, theServer);
        }

        
        /// <summary>
        /// Callback for when a receive operation completes (see BeginReceive)
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            Console.WriteLine("On Receive callback executing. ");
            SocketState theServer = (SocketState)ar.AsyncState;
            int numBytes = theServer.theSocket.EndReceive(ar);

            string message = Encoding.UTF8.GetString(theServer.messageBuffer, 0, numBytes);

            Console.WriteLine($"   Received {message.Length} characters.  Could be a message (or not) based on protocol");
            Console.WriteLine($"     Data is: {message}");

            theServer.sb.Append(message);

            ProcessMessages(theServer.sb);

            // Continue the "event loop" and receive more data
            theServer.theSocket.BeginReceive(theServer.messageBuffer, 0, theServer.messageBuffer.Length, SocketFlags.None, 
                OnReceive, theServer);
        }


        /// <summary>
        /// Look for complete messages (terminated by a '.'), 
        /// then print and remove them from the string builder.
        /// </summary>
        /// <param name="sb"></param>
        private void ProcessMessages(StringBuilder sb)
        {
            string totalData = sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\.])");

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // Ignore last message if incomplete
                if (p[p.Length - 1] != '.')
                    break;

                // process p
                Console.WriteLine("message received");
                Console.WriteLine(p);

                sb.Remove(0, p.Length);

            }
        }

    }
}
