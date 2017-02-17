using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Robotics_2017.Drivers {
    public class ParallaxPingSimple {
        public delegate void RangeTakenDelegate(object sender, PingEventArgs e);

        public event RangeTakenDelegate RangeEvent;

        private readonly TristatePort _port;

        public ParallaxPingSimple(Cpu.Pin pin, int period, bool enabled) {
            _port = new TristatePort(pin, false, false, ResistorModes.Disabled);

        }

        private int GetDistance() {
            // First we need to pulse the port from high to low.
            _port.Active = true; // Put port in write mode
            _port.Write(true); // Pulse pin
            _port.Write(false);
            _port.Active = false; // Put port in read mode;    

            var lineState = false;

            // Wait for the line to go high, for start of pulse.
            while (lineState == false)
                lineState = _port.Read();

            var startOfPulseAt = DateTime.Now.Ticks; // Save start ticks.

            // Wait for line to go low.
            while (lineState)
                lineState = _port.Read();

            var endOfPulse = DateTime.Now.Ticks; // Save end ticks. 

            var ticks = (int) (endOfPulse - startOfPulseAt);

            return ticks/580;
        }

        /// <summary>
        ///     Initiates the pulse, and triggers the event
        /// </summary>
        public void TakeMeasurement() {
            var distance = GetDistance();

            RangeEvent?.Invoke(this, new PingEventArgs(distance));

        }
    }
}