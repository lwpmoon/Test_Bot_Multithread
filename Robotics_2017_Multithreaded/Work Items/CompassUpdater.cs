using System.Threading;
using Microsoft.SPOT.Hardware;
using Robotics_2017.Drivers;

namespace Robotics_2017.Work_Items {
    public class CompassUpdater {
        private readonly Compass _compass;

        private readonly WorkItem _workItem;
        private readonly int _delay;

        public CompassUpdater(Cpu.AnalogChannel channel, int sampleCount, int delay = 1000) {
            _compass = new Compass();
            _workItem = new WorkItem(CompassUpdate, false, true, true);

            _delay = delay;
        }

        private void CompassUpdate() {
            RobotState.SetHeading(_compass.ReadHeading());
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