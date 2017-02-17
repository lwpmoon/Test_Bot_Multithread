using System.Threading;
using Microsoft.SPOT;
using Robotics_2017.Drivers;
using Robotics_2017.Flight_Computer;
using Robotics_2017.Work_Items;


namespace Robotics_2017 {
    
    
    //debug packets instead of usb debug
    
    public static class Program {
       
        public static void Main() {
            //Post methods
            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer posted successfully. Beginning INIT.");
            
            //Initialize sensors, etc.
            
            Debug.Print("Starting stopwatch");
            Clock.Instance.Start();

           
            //Thread.Sleep(5000);
            Debug.Print("Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Debug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start();
            
            // Start sensor actions here.
            Debug.Print("Flight computer boot successful.");
        }

    }
}

