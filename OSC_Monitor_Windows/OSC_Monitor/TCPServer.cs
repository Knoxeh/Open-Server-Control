using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;


namespace OSC_Monitor
{
    class TCPServer : Program
    {
        private TcpListener _server;
        private Boolean _isRunning = false;
        private int users = 0;

        private List<IPAddress> whitelist = new List<IPAddress>(); 
        public TCPServer(int port)
        {

            //Register Whitelist
            whitelist.Add(IPAddress.Parse("127.0.0.1"));

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
                this.users += 1;
                int userID = this.users;
                Thread t = new Thread(() => HandleClient(newClient, userID));
                //

                IPAddress connIP = IPAddress.Parse(((IPEndPoint)newClient.Client.RemoteEndPoint).Address.ToString());
                if (this.whitelist.Contains(connIP))
                {
                     Console.WriteLine("[TCP SERVER] NEW CLIENT [ID:"+userID+"] ("+connIP.ToString()+")");
                     t.Name = "User ("+userID+")";
                    t.Start();
                }
   
            }
        }
        public void HandleClient(object obj, int userID)
        {

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
                    if (cStream.CanRead)
                    {
                        //Read the data header - tells the buffer how much data is coming.
                        Byte[] msgLength = new Byte[9];
                        cStream.Read(msgLength, 0, msgLength.Length);
                        data = System.Text.Encoding.ASCII.GetString(msgLength,0,msgLength.Length);
                    
                        //Read the rest of the data depending on how much the header tells us there is.
                        data.Replace("\\","");
                        int msgLen = 0;
                        try {msgLen = Int32.Parse(data);}
                        catch(FormatException)
                        {
                            cStream.Close();
                            clientConnected = false;                            
                        }

                        if(cStream.CanRead)
                        {
  
                            Byte[] msgInput = new Byte[msgLen];
                            cStream.Read(msgInput, 0, msgInput.Length);
                            data = System.Text.Encoding.ASCII.GetString(msgInput, 0, msgInput.Length);

                            HandleClientInput(client, data);
                        }
                        
                    
                    }
      
                    

                    
                }
                catch (IOException exception)
                {
                    clientConnected = false;
                }  
            }

            if(!clientConnected)
            {
                  Console.WriteLine("USER [ID:"+userID+"] has disconnected..");
                  this.users -= 1;
            }

        }   
        public void HandleClientInput(object obj, String inData)
        {
            TcpClient client = (TcpClient)obj;
            
            Console.WriteLine("RECEIVED DATA: {0}", inData);
         
            inputCommand command = JsonConvert.DeserializeObject<inputCommand>(inData);

            Console.WriteLine("COMMAND RECEIVED: {0} {1}", command.Function, command.Args["id"]);
            
            switch(command.Function)
            {
                case "StartServer":
                    srvMgr.startServer(Int32.Parse(command.Args["id"].ToString()));
                    break;
                case "StopServer":
                    srvMgr.stopServer(Int32.Parse(command.Args["id"].ToString()));
                    break;

            }

        }
    }
}
