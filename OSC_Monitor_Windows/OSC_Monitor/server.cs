using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace OSC_Monitor
{
    public class server
    {
        //Initiate the server variables
        private String srvLoc { get; set; }
        private String srvExe { get; set; }
        private String srvParams { get; set; }
        public Boolean srvRunning { get; set; }
        private int srvPID { get; set; }
        public Process srvProcess { get; set; }
        public bool ProcessExists(int id) { return Process.GetProcesses().Any(x => x.Id == id); }
        public StreamReader consoleReader;
        public StreamWriter consoleWriter;
        public String consoleLine;
        Thread consoleHandler;
        //Initiate the server class and set the variables
        public server(string location, string exe, string paramaters, bool start)
        {
            srvLoc = location;
            srvExe = exe;
            srvParams = paramaters;
            srvPID = -1;

            //Setting up the new process 
            srvProcess = new Process();
            srvProcess.StartInfo.FileName = srvLoc + "/" + srvExe;
            srvProcess.StartInfo.Arguments = srvParams;
            srvProcess.StartInfo.UseShellExecute = false;
            //Allow the redirection of output and input for us to control the server
            srvProcess.StartInfo.RedirectStandardOutput = true;
            srvProcess.StartInfo.RedirectStandardInput = true;

            consoleWriter = srvProcess.StandardInput;
            
            //Create a thread to handle the console output
            consoleHandler = new Thread(() => HandleConsoleOutput());
            if (start)
                this.start(); 

        }
        //Handles console output
        public void HandleConsoleOutput()
        {
            using (StreamReader reader = srvProcess.StandardOutput)
            {
                while(!reader.EndOfStream)
                {
                    consoleLine = reader.ReadLine();
                  
                }
                

            }
        }
        //Starts the server - returns a boolean if it is successfull or not.
        public bool start()
        {   

            try
            {
                if(!srvRunning)
                {
                    srvProcess.Start();
                    consoleHandler.Start();
                    srvPID = srvProcess.Id;
                    consoleHandler.Name = "Console Handle " + srvPID + "";
                    this.consoleReader = srvProcess.StandardOutput;
                }
                else
                {
                    return false;
                }
                srvRunning = true;
            }
            catch (Win32Exception e)
            {
                Console.WriteLine("Something fucked (" + e.Message + ")");
                return false;
            }
              
            return true;
        }

        //Stops the server - returns a boolean if it is successful or not.
        public bool stop()
        {
            try
            {
                if (ProcessExists(srvPID) || srvPID != -1) //If the process hasn't been started or it doesnt even exist
                {
                    if(srvRunning)
                    {
                        srvProcess.Kill();
                        consoleHandler.Abort();
                        srvPID = -1;
                        srvRunning = false;
                    }
                    else
                    {
                        return false;
                    }
                  
                }
                else
                {
                    Console.WriteLine("Server not running - cannot stop a ghost!");
                    return false;
                }
            }
            catch (Win32Exception e)
            {
                Console.WriteLine("Something fucked (" + e.Message + ")");
                return false;
            }

            return true;
        }

        //Restarts the server - stops if it is unsuccssful at ANY step.
        public bool restart()
        {
            Boolean rStart = false, rStop = false;

            if (ProcessExists(srvPID) || srvPID != -1) //If the process hasn't been started or it doesnt even exist
            {
                rStop = this.stop();
                if (rStop)
                    rStart = this.start();
                if (rStart)
                    return true;
            }
            else
                Console.WriteLine("Server not running - cannot restart.");
            return false;

        }
        public int getPID()
        {
            return srvPID;
        }
    }
}
