using System.Threading;
using Microsoft.SPOT.Hardware;
using Test_Bot_Multithread.Drivers;

namespace Test_Bot_Multithread.Work_Items {
    public class IRDistanceUpdater {
        private readonly IrDistanceSensor _irSensor;

        private readonly WorkItem _workItem;
        private readonly int _delay;

        public IRDistanceUpdater(Cpu.AnalogChannel channel, int sampleCount, int delay = 1000) {
            _irSensor = new IrDistanceSensor(channel, sampleCount);
            _workItem = new WorkItem(BroadcastIrUpdate, false, true, true);

            _delay = delay;
        }

        private void BroadcastIrUpdate() {
            RobotState.SetIrDistance(_irSensor.ReadDistance());
            Thread.Sleep(_delay);
        }

        public void Start() {
            _workItem.Start();
        }

        public void Stop() {
            _workItem.Stop();
        }
    }
}