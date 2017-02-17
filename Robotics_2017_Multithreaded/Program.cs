using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Robotics_2017.Drivers;
using Robotics_2017.Flight_Computer;
using Robotics_2017.Work_Items;
using SecretLabs.NETMF.Hardware.Netduino;

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
            
            var _motors = new MotorController();

            var testPing = new PingUpdater(Pins.GPIO_PIN_A0);
            testPing.Start();

            var testIR = new IRDistanceUpdater(AnalogChannels.ANALOG_PIN_A1,25,100);
            testIR.Start();

            while (true) {
                Debug.Print("IR: " + RobotState.IRDistance);
                Debug.Print("\nPing: " + RobotState.PingDistance);
                Thread.Sleep(1000);
                
            }

            // Start sensor actions here.
            Debug.Print("Flight computer boot successful.");
        }

    }
}

