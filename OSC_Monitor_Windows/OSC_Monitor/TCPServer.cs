using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace OSC_Monitor
{
    class TCPServer
    {
        private TcpListener _server;
        private Boolean _isRunning = false;

        public TCPServer(int port)
        {
            //Initiate the TCPListener to listen on any IP, and the specific port.
            _server = new TcpListener(IPAddress.Any, port);
            
            Console.WriteLine("[TCP SERVER] Monitor listening on port {0}", port);
            _server.Start();

            _isRunning = true;

            LoopClients();
            
        }
        public void LoopClients()
        {
            while(_isRunning) //Keep receiving clients while the server is running.
            {
                //New Connection Received, now we will start a thread to handle that client (this allows for multiple clients)
                TcpClient newClient = _server.AcceptTcpClient(); 
                
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }
        public void HandleClient(object obj)
        {

            Console.WriteLine("[TCP SERVER] NEW CLIENT");

            //Get the client object, and the IP from that object.
            TcpClient client = (TcpClient)obj;
            IPAddress clientIP = IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());

            Boolean clientConnected = true;
            NetworkStream cStream = client.GetStream();

            String data = null;

            while(clientConnected)
            {
                try
                {
                    //Read the data header - tells the buffer how much data is coming.
                    Byte[] msgLength = new Byte[11];
                    cStream.Read(msgLength, 0, msgLength.Length);
                    data = System.Text.Encoding.ASCII.GetString(msgLength,0,msgLength.Length);
                   
                    //Read the rest of the data depending on how much the header tells us there is.
                    int msgLen = Int32.Parse(data);
                    Byte[] msgInput = new Byte[msgLen];
                    cStream.Read(msgInput, 0, msgInput.Length);
                    data = System.Text.Encoding.ASCII.GetString(msgInput, 0, msgInput.Length);
                    
                    HandleClientInput(client, data);

                    
                }
                catch (IOException exception)
                {
                    Console.WriteLine("Client Forcefully Close Stream.");
                    clientConnected = false;
                }  
            }
        }
        public void HandleClientInput(object obj, String inData)
        {
            TcpClient client = (TcpClient)obj;

            Console.Write("RECEIVED DATA: {0}", inData);


        }
    }
}
