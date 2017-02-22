using System.Threading;
using Microsoft.SPOT.Hardware;
using test_bot_netduino;
using Test_Bot_Multithread.Drivers;

namespace Test_Bot_Multithread.Work_Items
{
    public class ReceiverUpdater
    {
        private readonly Receiver _receiver;

        private readonly WorkItem _workItem;
        //private readonly WorkItem _workItem2;
        private readonly int _delay;

        //Maximum refresh rate from the HMC3883L is 14ms in continuous measurement mode
        public ReceiverUpdater(int delay = 500)
        {
            _receiver = new Receiver(1000);
            _workItem = new WorkItem(CompassUpdate, false, true, true);
            //_workItem2 = new WorkItem(Health, false, true, true);

            _delay = delay;
        }

        private void CompassUpdate()
        {
            RobotState.SetBearing(_receiver.findBeacon());
            Thread.Sleep(_delay);
        }

        private void Health()
        {
            RobotState.SetBeaconHealth(_receiver.beaconPresent());
            Thread.Sleep(_delay);
        }

        public void Start()
        {
            _workItem.Start();
            //_workItem2.Start();
        }

        public void Stop()
        {
            _workItem.Stop();
            //_workItem2.Stop();
        }
    }
}