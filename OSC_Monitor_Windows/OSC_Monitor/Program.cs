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
        


        


        static void Main(string[] args)
        {

            printLicense();
            string sqlConnStr = @"server=localhost;userid=root;password=password;database=osc_panel";
            serverManager srvMgr = new serverManager();
            MySqlConnection sqlConn = null;
            MySqlDataReader sqlReader = null;
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
                   /*Console.WriteLine(sqlReader.GetInt32(0) + ": "
                        + sqlReader.GetString(1) + ": "
                        + sqlReader.GetString(2) + ": "
                        + sqlReader.GetString(3));
                    */
                    srvMgr.addServer(new server(sqlReader.GetString(1), sqlReader.GetString(2), sqlReader.GetString(3), true));
                    serverCount++;
                }

            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error {0}", ex.ToString());
            }
            finally
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
            /*Console.WriteLine("Let's start the cluckles!");
            server testServer = new server("C:/Users/Nolan/Documents/GitHub/Open-Server-Control/OSC_Monitor_Windows/OSC_Monitor", "cluckles.exe", "", true);

            Thread.Sleep(2000);

            Console.WriteLine("Let's restart cluckles!");

            testServer.restart();

            

            Console.WriteLine("Let's stop cluckles!");

            testServer.stop();
            */
            Thread.Sleep(60000);
            
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
