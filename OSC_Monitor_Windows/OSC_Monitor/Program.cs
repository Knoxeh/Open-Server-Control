using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Net.Sockets;


namespace OSC_Monitor
{

    class Program
    {

        static MySqlConnection sqlConn = null;
        static MySqlDataReader sqlReader = null;
        private TcpListener _server;
        static void Main(string[] args)
        {


            printLicense();
            string sqlConnStr = @"server=localhost;userid=root;password=******;database=ogp_panel";

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
            string hashConStr = "6367c48dd193d56ea7b0baad25b19455e529f5ee"; //Hardcoded hashstring - this will be configurable..
            
            string replyMessage = hashConStr + "| DEFAULT MESSAGE - CONTACT ADMINISTRATOR."; // default message - this should never be output

            TCPServer tcs = new TCPServer(13000);
           
        }
        public static void printLicense()
        {
            Console.Write("Copyright (C) 2014  Nolan 'Knoxeh' Murphy\n\nThis program is free software; you can redistribute it and/or modify it under\nthe terms of the GNU General Public License as published by the Free Software\nFoundation; either version 2 of the License, or (at your option) any later\nversion.\n\nThis program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.\n\nYou should have received a copy of the GNU General Public License along with\nthis program; if not, write to the Free Software Foundation, Inc., 51 Franklin\nStreet, Fifth Floor, Boston, MA  02110-1301, USA.\n");
            Console.WriteLine("--------------------------------------------------------------------------------");
        }
        public static void printConsole(String msg)
        {
            Console.WriteLine(msg);
        }
    }
}
