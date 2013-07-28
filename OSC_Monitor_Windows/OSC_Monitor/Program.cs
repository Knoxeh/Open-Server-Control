using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace OSC_Monitor
{

    class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine("Let's start the cluckles!");
            server testServer = new server("C:/Users/Nolan/Documents/GitHub/Open-Server-Control/OSC_Monitor_Windows/OSC_Monitor", "cluckles.exe", "", true);

            Thread.Sleep(2000);

            Console.WriteLine("Let's restart cluckles!");

            testServer.restart();

            Thread.Sleep(2000);

            Console.WriteLine("Let's stop cluckles!");

            testServer.stop();

            
        }
        
        public static void printConsole(String msg)
        {
            Console.WriteLine(msg);
        }
    }
}
