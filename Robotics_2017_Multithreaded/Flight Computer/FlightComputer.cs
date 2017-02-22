﻿using Test_Bot_Multithread.Work_Items;

namespace Test_Bot_Multithread.Flight_Computer {
    public class FlightComputer {
    
        private static FlightComputer _instance;
        private static readonly object Locker = new object();

        public static FlightComputer Instance
        {
            get
            {
                lock(Locker)
                    return _instance ?? (_instance = new FlightComputer());
            }
        }

        //public static bool Launched { get; set; }
        //public static Logger Logger { get; set; }

        private FlightComputer()
        {
            //Launched = false;
        }

        public void Execute(WorkItem workItem) {
            ThreadPool.QueueWorkItem(workItem);
        }
        public static event EventTriggered OnEventTriggered;

        public delegate void EventTriggered(bool loggable, ref byte[] arrayData);

        public void TriggerEvent(bool loggable, ref byte[] arrayData) {

            OnEventTriggered?.Invoke(loggable, ref arrayData);
        }
    }

  
}

