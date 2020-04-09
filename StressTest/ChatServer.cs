using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CS3500
{
    

    /// <summary>
    /// A simple server for sending simple text messages to multiple clients
    /// </summary>
    public class ChatServer
    {
        /// <summary>
        /// keep track of how big a message to send... keep getting bigger!
        /// </summary>
        private long larger = 5000;

        /// <summary>
        /// A list of all clients currently connected
        /// </summary>
        private List<Socket> clients = new List<Socket>();

        private TcpListener listener;

        /// <summary>
        ///   Nothing to "construct" at this time
        /// </summary>
        public ChatServer()
        {
        }

        /// <summary>
        /// Start accepting Tcp socket connections from clients
        /// </summary>
        /// <param name="portStr">port we want to server listen on</param>
        public void StartServer(string portStr = "11000")
        {
            int port;
           
            if (! Int32.TryParse(portStr, out port) )
            {
                port = 11000;
            }

            listener = new TcpListener(IPAddress.Any, port);
            Console.WriteLine($"Server waiting for clients here: 127.0.0.1 on port {port}");

            listener.Start();

            // This begins an "event loop".
            // ConnectionRequested will be invoked when the first connection arrives.
            // TODO: we should be passing the TcpListener as the last argument, instead 
            //       of having it as a member of the class.
            listener.BeginAcceptSocket(ConnectionRequested, null);
        }

        /// <summary>
        /// A callback for invoking when a socket connection is accepted
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectionRequested(IAsyncResult ar)
        {
            Console.WriteLine("Contact from client");

            // Get the socket
            clients.Add(listener.EndAcceptSocket(ar));

            // continue an event-loop that will allow more clients to connect
            listener.BeginAcceptSocket(ConnectionRequested, null);
        }


        /// <summary>
        /// Continuously ask the user for a message to send to the client
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(string message)
        {
            if (message == "largemessage")
            {
                message = GenerateLargeMessage();
            }

            //
            // Begin sending the message
            //
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            List<Socket> toRemove = new List<Socket>();

            Console.WriteLine($"   Sending a message of size: {message.Length}");

            foreach (Socket s in clients)
            {
                try
                {
                    s.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, s);
                }
                catch (Exception) // Begin Send fails if client is closed
                { 
                    toRemove.Add(s); 
                }
            }

            // update list of "current" clients by removing closed clients
            foreach (Socket s in toRemove)
            {
                Console.WriteLine($"Client {s} disconnected");
                clients.Remove(s);
            }
        }

        /// <summary>
        /// Generate a big string of the letter a repeated...
        /// </summary>
        /// <returns></returns>
        private string GenerateLargeMessage()
        {
            StringBuilder retval = new StringBuilder();

            for (int i = 0; i < larger; i++)
                retval.Append("a");
            retval.Append(".");

            larger += larger; 

            return retval.ToString();
        }


        /// <summary>
        /// A callback invoked when a send operation completes
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            // 
            //   This code could be useful to update the state of a program once you know a client
            //   has received some information, for example, if you had a counter of successfully sent 
            //   messages you would increment it here.
            //
            Socket client = (Socket)ar.AsyncState;
            long send_length = client.EndSend(ar);

            Console.WriteLine($"   Sent a message of size: {send_length}");

        }

    }
}