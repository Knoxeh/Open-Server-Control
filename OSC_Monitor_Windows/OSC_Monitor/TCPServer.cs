using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace OSC_Monitor
{
    class TCPServer : Program
    {
        private TcpListener _server;
        private Boolean _isRunning = false;
        private Boolean clientConnected;
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

            //Constantly be ready to accept more clients
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

                //Setup the client thread (Allows for the program to handle each client seperately 
                Thread t = new Thread(() => HandleClient(newClient, userID));
                t.Name = "Client Thread"; //Give thread a name for helpful debugging
               

                IPAddress connIP = IPAddress.Parse(((IPEndPoint)newClient.Client.RemoteEndPoint).Address.ToString());
                //Get the clients I
                if (this.whitelist.Contains(connIP))
                {
                     Console.WriteLine("[TCP SERVER] NEW CLIENT [ID:"+userID+"] ("+connIP.ToString()+")");
                     
                    t.Start();
                }
   
            }
        }
        public void HandleClient(object obj, int userID)
        {

            //Get the client object
            TcpClient client = (TcpClient)obj;

            //Network stream - allows for read/write - each client has one of these
            NetworkStream cStream;
            cStream = client.GetStream();

            String data = null;
           
            while(client.Connected) //Keep the session going as long as the client is connected - sometimes we will have a quick one line message or continuing output
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
                            //Read what the client has sent us
                            Byte[] msgInput = new Byte[msgLen];
                            cStream.Read(msgInput, 0, msgInput.Length);
                            data = System.Text.Encoding.ASCII.GetString(msgInput, 0, msgInput.Length);
                            //Send to input handler where it would distinguish the commands and work accordingly
                            HandleClientInput(client, data, cStream);
                        }
                        
                    
                    }
      
                    

                    
                }
                catch (IOException exception)
                {
             
                    
                }  
            }

            if(!client.Connected)
            {
                  Console.WriteLine("USER [ID:"+userID+"] has disconnected..");
                  this.users -= 1;
            }

        }

        //Receive messages from the client and act accordingly
        public void HandleClientInput(object obj, String inData, NetworkStream cStream)
        {
            //Get client object
            TcpClient client = (TcpClient)obj;


            //Get the command and generally put out some debug outputs for the received input - most likely these console prints will be removed once a usable version is there.
            Console.WriteLine("RECEIVED DATA: {0}", inData);

            inputCommand command = JsonConvert.DeserializeObject<inputCommand>(inData);

            Console.WriteLine("COMMAND RECEIVED: {0} {1}", command.Function, command.Args["id"]);


            JObject jsonReponse;
            //Starts the server by ID, and returns whether it was successfull or not
            if (command.Function == "StartServer")
            {
                Boolean commandResponse = srvMgr.startServer(Int32.Parse(command.Args["id"].ToString()));
                jsonReponse = JObject.FromObject(new
                {
                    success = commandResponse
                });

                outputResponse linqResponse = new outputResponse(command.Function, command.Args, jsonReponse);
                string response = JsonConvert.SerializeObject(linqResponse);

                Byte[] data = System.Text.Encoding.ASCII.GetBytes(response);
                Byte[] length = System.Text.Encoding.ASCII.GetBytes(response.Length.ToString("D9"));

                cStream.Write(length, 0, length.Length);
                cStream.Write(data, 0, data.Length);
            }
            //Requests the console's output and sends the cleint said output
            else if (command.Function == "GetConsoleOutput")
            {
                Thread t = new Thread(() => handleConsoleOutput(client, command.Args));
                t.Name = "Console Output Thread";
                t.Start();

            }
            //Stops server
            else if (command.Function == "StopServer")
            {
                srvMgr.stopServer(Int32.Parse(command.Args["id"].ToString()));
            }
            //Unwritten - will send the client a list of the servers they have access to (admins will see all)
            else if (command.Function == "GetServers")
            {

            }


        }
        //When requested this function will be started as it's own thread to just output the consoles output
        public void handleConsoleOutput(object obj, JObject InArgs)
        {

            //Get the client object, and the IP from that object.
            string consoleLine = "null"; 
            JObject jsonReponse;

            //Get client and the clients stream
            TcpClient client = (TcpClient)obj;
            NetworkStream cStream = client.GetStream();

            //Keep outputting console while client is connected
            while (client.Connected)
            {   
                string newconsoleLine = srvMgr.getServer(Convert.ToInt32(InArgs["id"])).consoleLine;

                if (newconsoleLine != consoleLine) //Only broadcast to the client if the message is actually new
                {
                    consoleLine = newconsoleLine;

                    //No longer need to debug the output from the server
                    //Console.WriteLine(consoleLine);

                    jsonReponse = JObject.FromObject(new
                    {
                        success = consoleLine
                    });

                    outputResponse linqResponse = new outputResponse("ConsoleOutput", InArgs, jsonReponse);
                    string response = JsonConvert.SerializeObject(linqResponse);

                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(response);
                    Byte[] length = System.Text.Encoding.ASCII.GetBytes(response.Length.ToString("D9"));
                    try //Send the message
                    {
                        cStream.Write(length, 0, length.Length);

                        cStream.Write(data, 0, data.Length);


                    }
                    catch (Exception exception)
                    {
                        // client.Close();

                    }
                }

            }
           
           
        }
    }
}
