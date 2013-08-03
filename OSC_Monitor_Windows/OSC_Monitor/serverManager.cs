﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;

namespace OSC_Monitor
{
    class serverManager
    {
        const int checkInterval = 10000;
        private static System.Timers.Timer srvCheckTimer;
        private static List<server> serverList;

        public static bool ProcessExists(int id) { return Process.GetProcesses().Any(x => x.Id == id); }
        
        public serverManager()
        {

            //Initialize Server ArrayList
            serverList = new List<server>();
            
            //Create a timer
            srvCheckTimer = new Timer(checkInterval);

            //Ticky the tock tock
            srvCheckTimer.Elapsed += new ElapsedEventHandler(OnServerCheckEvent);
            srvCheckTimer.Start();
        }
        //Server check event - ran ever 10 seconds to check if each server is running.
        private static void OnServerCheckEvent(object source, ElapsedEventArgs e)
        {
            
            for(int x = 0; x < serverList.Count; x++)
            {
                if(!ProcessExists(serverList[x].getPID()) && serverList[x].getPID() != -1)
                {
                    Console.WriteLine("Server PID " + serverList[x].getPID() + " has crashed, restarting..");
                    serverList[x].start();
                }
                
            }
            
        }
        public void addServer(server newServer)
        {
            serverList.Add(newServer);
            
        }
        public server getServer(int id)
        {
            return serverList[id];
        }
        public void removeServer(int id)
        {

            if (ProcessExists(serverList[id].getPID()))
                serverList[id].stop();

            serverList.RemoveAt(id);

        }
    }
}
