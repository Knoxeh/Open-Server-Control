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
            _server = new TcpListener(IPAddress.Any, port);

            Console.WriteLine("[TCP SERVER] Monitor listening on port {0}", port);
            _server.Start();

            _isRunning = true;

            LoopClients();
            
        }
        public void LoopClients()
        {
            while(_isRunning)
            {

                TcpClient newClient = _server.AcceptTcpClient();
                Console.WriteLine("NEW CLIENT");
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }
        public void HandleClient(object obj)
        {
            

            TcpClient client = (TcpClient)obj;
            IPAddress clientIP = IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());

            Boolean clientConnected = true;
            NetworkStream cStream = client.GetStream();

            String data = null;

            while(clientConnected)
            {
                try
                {
                    Byte[] msgLength = new Byte[10];
                    
                    cStream.Read(msgLength, 0, msgLength.Length);
                    data = System.Text.Encoding.ASCII.GetString(msgLength, 0, 10);
                    //Console.WriteLine("Received: {0}", data);
                    int msgLen = Int32.Parse(data);
                    Byte[] msgInput = new Byte[msgLen];
                    cStream.Read(msgInput, 0, msgInput.Length);
                    data = System.Text.Encoding.ASCII.GetString(msgInput, 11, msgInput.Length);
                    Console.WriteLine("Received: {0}", data);
                    

                    
                }
                catch (IOException exception)
                {
                    Console.WriteLine("Client Forcefully Close Stream.");
                    clientConnected = false;
                }  
            }
        }
    }
}
