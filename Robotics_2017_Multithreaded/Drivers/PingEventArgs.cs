namespace Robotics_2017.Drivers {
    public class PingEventArgs {
        public PingEventArgs(int distance) {
            Distance = distance;
        }

        public int Distance { get; private set; }
    }
}