using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using ZeroMQ;


namespace OSC_Monitor
{

    class Program
    {

        static MySqlConnection sqlConn = null;
        static MySqlDataReader sqlReader = null;
        static void Main(string[] args)
        {


            printLicense();
            string sqlConnStr = @"server=localhost;userid=root;password=password;database=osc_panel";

            //Initiate serverManger, it handles crashed servers
            serverManager srvMgr = new serverManager();
            
            //Load servers from the database.
            try
            {
                sqlConn = new MySqlConnection(sqlConnStr);
                sqlConn.Open();
                Console.WriteLine("CONNECTED TO DATABASE MySQL version : {0}", sqlConn.ServerVersion);

                string sqlQuery = "SELECT * FROM osc_servers";
                MySqlCommand sqlCmd = new MySqlCommand(sqlQuery, sqlConn);

                sqlReader = sqlCmd.ExecuteReader();
                int serverCount = 0;
                while (sqlReader.Read())
                {
                    srvMgr.addServer(new server(sqlReader.GetString(1), sqlReader.GetString(2), sqlReader.GetString(3), true));
                    serverCount++;
                }
                Console.WriteLine("Loaded {0} server(s) from database.", serverCount.ToString());
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error {0}", ex.ToString());
            }
            finally //Once the mysql connection is complete we can close it.
            {
                if (sqlReader != null)
                {
                    sqlReader.Close();
                }
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            //Let's do some ZMQ Magic.
            Console.WriteLine("Listening monitor server to port 5555");
            string hashConStr = "6367c48dd193d56ea7b0baad25b19455e529f5ee"; //Hardcoded hashstring - this will be configurable..
            
            string replyMessage = hashConStr + "| DEFAULT MESSAGE - CONTACT ADMINISTRATOR."; // default message - this should never be output

            //Start teh ZMQ context and wait for some messages from the client
            using (var context = ZmqContext.Create())
            {
                using (ZmqSocket zmqReplyer = context.CreateSocket(SocketType.REP)) //Let's use a replyer for now - might change depending what i neeed.
                {
                    zmqReplyer.Bind("tcp://*:5555");
                    while (true)
                    {
                        try
                        {
                            
                            string zmqRecvMsg = zmqReplyer.Receive(Encoding.Unicode); //Receive message
                            string[] cmdSplit = zmqRecvMsg.Split("|".ToCharArray()); //Split message in readable chunks - 0: hashed connection string | 1: command | 2: target
                            if (cmdSplit[0].Equals(hashConStr))
                            {

                                int cmdNum = Convert.ToInt32(cmdSplit[1]);
                                int targServ = Convert.ToInt32(cmdSplit[2]);

                                //Handle the commands
                                bool cmdState = false;
                                if (cmdNum == 1) //Start
                                {
                                    cmdState = srvMgr.getServer(targServ).start();
                                }
                                else if (cmdNum == 2)//Stop
                                {
                                    cmdState = srvMgr.getServer(targServ).stop();
                                }
                                else if (cmdNum == 3)//Restart
                                {
                                    cmdState = srvMgr.getServer(targServ).restart();
                                }

                                if (cmdState) //If the commands are executed sucessfully
                                    replyMessage = hashConStr + "| COMMAND EXECUTED SUCESSFULLY";
                                else
                                    replyMessage = hashConStr + "| COMMAND EXECUTED UNSUCESSFULLY";
                                // Send reply back to client
                                zmqReplyer.Send(replyMessage, Encoding.Unicode); //Reply!
                            }
                            else
                                zmqReplyer.Send("CREDENTIALS REJECTED.", Encoding.Unicode);

                        }
                        catch (Exception ex)//error handling - make sure evil people can't send commands and make sure they are in a correct structure.
                        {
                            if (ex is IndexOutOfRangeException)
                            {
                                Console.WriteLine("Invalid request received.");
                                zmqReplyer.Send("INVALID MESSAGE", Encoding.Unicode);

                            }
                            else if (ex is ArgumentOutOfRangeException)//Make sure the string has the expected structure!
                            {
                                zmqReplyer.Send("INVALID COMMAND STRUCTURE", Encoding.Unicode);
                            }
                            else//If there isn't a handled error - vomit at the client.
                                zmqReplyer.Send("Something went wrong: " + ex.ToString(), Encoding.Unicode);

                        }

                    }
                }
            }
        }
        public static void printLicense()
        {
            Console.WriteLine("Open Server Control, Copyright (C) 2013");
            Console.WriteLine("Open Server Control comes with ABSOLUTELY NO WARRANTY");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions; type `show c' for details.");
            Console.WriteLine("--------------------------------------------------------------------------------");
        }
        public static void printConsole(String msg)
        {
            Console.WriteLine(msg);
        }
    }
}
