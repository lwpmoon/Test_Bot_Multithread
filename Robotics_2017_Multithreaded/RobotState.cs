using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Robotics_2017.Work_Items {
    public class MotorController {
        private const double Frequency = 490;

        private readonly OutputPort _leftDir = new OutputPort(Pins.GPIO_PIN_D12, false);
        private readonly PWM _leftPWM = new PWM(PWMChannels.PWM_PIN_D3, Frequency, 1, false);
        private readonly OutputPort _rightDir = new OutputPort(Pins.GPIO_PIN_D13, false);
        private readonly PWM _rightPWM = new PWM(PWMChannels.PWM_PIN_D11, Frequency, 1, false);
        public void Forward(int s)
        {
            if (RobotState.IsEnabled() && (s <= 255))// || (s > 0))
            {


                _leftDir.Write(true);
                _rightDir.Write(true);

                _leftPWM.DutyCycle = (s / 255);
                _rightPWM.DutyCycle = (s / 255);

                _leftPWM.Start();
                _rightPWM.Start();
            }
            else Halt();
        }

        public void Backward(int s)
        {
            if (s <= 255 && RobotState.IsEnabled())
            {
                Halt();

                _leftDir.Write(false);
                _rightDir.Write(false);
                
                _leftPWM.DutyCycle = (s / 255);
                _rightPWM.DutyCycle = (s / 255);
                
                _leftPWM.Start();
                _rightPWM.Start();
            }
            else Halt();
        }

        public void Right(int s)
        {
            if (RobotState.IsEnabled())
            {
                _leftDir.Write(true);
                _rightDir.Write(false);
                
                _leftPWM.DutyCycle = (s / 255);
                _rightPWM.DutyCycle = (s / 255);
                
                _leftPWM.Start();
                _rightPWM.Start();
            }
            else Halt();
        }
        public void Halt()
        {
            _leftPWM.Stop();
            _rightPWM.Stop();
        }
    }

    public static class RobotState {

        public static int PingDistance { get; private set; }
        public static int LastPingDistance { get; private set; }
        public static double IRDistance { get; private set; }
        public static double LastIRDistance { get; private set; }
        public static double LastcompassHeading { get; private set; }
        public static double CompassHeading { get; private set; }

        private static readonly AnalogInput enablePin = new AnalogInput(AnalogChannels.ANALOG_PIN_A0 );


        static RobotState() {
            PingDistance = int.MaxValue;
            LastPingDistance = int.MaxValue;
            IRDistance = int.MaxValue;
            LastIRDistance = int.MaxValue;
            CompassHeading = int.MaxValue;
            LastcompassHeading = int.MaxValue;
        }

        public static void SetPingDistance(int distance) {
            LastPingDistance = PingDistance;
            PingDistance = distance;
        }

        public static void SetIrDistance(double distance) {
            LastIRDistance = IRDistance;
            IRDistance = distance;
        }

        public static void SetHeading(double heading)
        {
            LastcompassHeading = CompassHeading;
            CompassHeading = heading;
        }
        public static bool IsEnabled() {
            return enablePin.Read() >= 0.9;
        }
    }
}