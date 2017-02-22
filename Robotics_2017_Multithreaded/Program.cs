using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Test_Bot_Multithread.Drivers;
using Test_Bot_Multithread.Flight_Computer;
using Test_Bot_Multithread.Work_Items;
using SecretLabs.NETMF.Hardware.Netduino;
using test_bot_netduino;
using Math = System.Math;

namespace Test_Bot_Multithread {
    
    
    //debug packets instead of usb debug
    
    public static class Program {
        static OutputPort RedLED = new OutputPort(Pins.GPIO_PIN_D4, true);
        static OutputPort YellowLED = new OutputPort(Pins.GPIO_PIN_D5, true);
        static OutputPort GreenLED = new OutputPort(Pins.GPIO_PIN_D6, true);
        static OutputPort BlueLED = new OutputPort(Pins.GPIO_PIN_D7, true);

        //Only 100 and 400 are supported values at this time
        public const int clockSpeed = 100;

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
            
            //var _motors = new MotorController();

            //var testPing = new PingUpdater(Pins.GPIO_PIN_A0);
            //testPing.Start();

            //var testIR = new IRDistanceUpdater(AnalogChannels.ANALOG_PIN_A1,25,100);
            //testIR.Start();

            var testCompass = new CompassUpdater();
            testCompass.Start();

            var testReceiver = new ReceiverUpdater();
            testReceiver.Start();
            

            // Start sensor actions here.
            Debug.Print("Flight computer boot successful.");

            while (true)
            { 
                Debug.Print("Degrees: " + RobotState.CompassHeading);
                //Debug.Print("Bearing: " + RobotState.Bearing);
                //if(!RobotState.BeaconPresent)Debug.Print("Beacon not present...");
                
                Thread.Sleep(1000);
            }


            //while (true)
            //{
            //    Debug.Print("IR: " + RobotState.IRDistance);
            //    Debug.Print("\nPing: " + RobotState.PingDistance);
            //    Thread.Sleep(1000);
            //    var oldSenV = RobotState.LastIRDistance;
            //    var currentSenV = RobotState.IRDistance;

            //    GreenLED.Write(RobotState.IsEnabled());

            //    BlueLED.Write(currentSenV >= 1000);
            //    YellowLED.Write(currentSenV >= 2000);
            //    if (currentSenV >= 1000) BlueLED.Write(true);

            //    if (Math.Abs(oldSenV - currentSenV) > .01)
            //    {
            //        Debug.Print("SensorValue: " + currentSenV);

            //        RedLED.Write(false);
            //        YellowLED.Write(false);
            //        BlueLED.Write(false);

            //        if (currentSenV >= 1000) BlueLED.Write(true);
            //        if (currentSenV >= 2000) YellowLED.Write(true);
            //        if (!(currentSenV >= 3000)) continue;

            //        RedLED.Write(true);
            //        _motors.Halt();

            //        _motors.Backward(255);
            //        Thread.Sleep(100);

            //        if (currentSenV >= 2000)
            //        {
            //            //do nothing
            //            Debug.Print("Too close...");
            //            _motors.Halt();
            //            _motors.Right(255);

            //        }
            //    }
            //}
        }
        
    }
}

