using Microsoft.SPOT.Hardware;
using Test_Bot_Multithread.Drivers;

namespace Test_Bot_Multithread.Work_Items {
    public class PingUpdater {

        //minimum period of 1000ms between pulses
        private readonly ParallaxPing _pingSensor;
        
        /// <summary>
        /// Create this to update global state with Parallax Ping info
        /// </summary>
        /// <param name="pin">the Cpu.Pin that the ping data is plugged into.</param>
        /// <param name="period">minimum of 1000 ms.</param>
        public PingUpdater(Cpu.Pin pin, int period = 1000) {
            _pingSensor = new ParallaxPing(pin, period, false);
        }

        public void OnPingUpdate(object sender, PingEventArgs latest) {
            RobotState.SetPingDistance(latest.Distance);
        }
        
        public void Start() {
            _pingSensor.Enabled = true;
            _pingSensor.RangeEvent += OnPingUpdate;
        }

        public void Stop() {
            _pingSensor.Enabled = false;
            _pingSensor.RangeEvent -= OnPingUpdate;
        }

        
    }
}