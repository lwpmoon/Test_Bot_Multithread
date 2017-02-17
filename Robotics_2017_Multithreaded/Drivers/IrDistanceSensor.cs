using System;
using Microsoft.SPOT.Hardware;

namespace Robotics_2017.Drivers {
    public class IrDistanceSensor
    {
        private AnalogInput _input;
        private double[] _data;

        public IrDistanceSensor(Cpu.AnalogChannel channel, int sampleCount)
        {
            _input = new AnalogInput(channel);
            _data = new double[sampleCount];
        }

        public double ReadDistance()
        {

            var total = 0d;
            var min = double.MaxValue;
            var max = double.MinValue;
            var count = _data.Length;

            for (int i = 0; i < count; i++)
            {
                var raw = _input.Read();
                _data[i] = raw;
                total += raw;

                if (raw < min) min = raw;
                if (raw > max) max = raw;

            }

            var average = total / count;
            var stDev = StandardDeviation(_data, average);

            if (stDev > 0.01)
            {
                var position = 0;
                min = average - stDev;
                max = average + stDev;


                for (int i = 0; i < count; i++)
                {
                    var data = _data[i];
                    if (data > min && data < max)
                        _data[position++] = data;
                }
                average = Average(_data, position);
            }
            var volts = average * 3.3;

            return 8.30330497051182 * Math.Pow(volts, 6d)
                    - 89.8292932369688 * Math.Pow(volts, 5d)
                    + 391.808954977875 * Math.Pow(volts, 4d)
                    - 885.170571942885 * Math.Pow(volts, 3d)
                    + 1106.23720332132 * Math.Pow(volts, 2d)
                    - 753.770168858126 * volts
                    + 250.173288592975;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _input.Dispose();
        }

        private static double StandardDeviation(double[] data, double avg)
        {
            var varTot = 0d;
            var max = data.Length;

            if (max == 0)
                return 0;

            for (var i = 0; i < max; i++)
            {
                var variance = data[i] - avg;
                varTot = varTot + (variance * variance);
            }

            return Math.Sqrt(varTot / max);
        }

        private static double Average(double[] data, int count)
        {
            var avg = 0d;

            for (var i = 0; i < count; i++)
                avg += data[i];

            if (avg == 0 || count == 0)
                return 0;

            return avg / count;
        }

        ~IrDistanceSensor()
        {
            Dispose();
        }

    }
}