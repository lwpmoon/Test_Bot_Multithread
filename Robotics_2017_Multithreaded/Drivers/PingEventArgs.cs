namespace Test_Bot_Multithread.Drivers {
    public class PingEventArgs {
        public PingEventArgs(int distance) {
            Distance = distance;
        }

        public int Distance { get; private set; }
    }
}