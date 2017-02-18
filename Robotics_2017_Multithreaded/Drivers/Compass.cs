using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using System.Diagnostics;
using System.Threading;
using Math = System.Math;

namespace Robotics_2017.Drivers
{
    public class Compass : IDisposable
    {
        private enum Adresses : byte
        {

            /// <summary>
            /// 7-bit I2C physical address of the HMC5883L
            /// </summary>
            HMC588L_ADDRESS = 0x1E,

            /// <summary>
            /// 7-bit I2C read address of the HMC5883L
            /// </summary>
            HMC588L_ADDRESS_READ = 0x3D,

            /// <summary>
            /// 7-bit I2C write address of the HMC5883L
            /// </summary>
            HMC588L_ADDRESS_WRITE = 0x3C
        }

        private enum Registers : byte
        {
            /// <summary>
            /// Configuration Register A (Read/Write)
            /// </summary>
            ConfigurationRegisterA = 0x00,

            /// <summary>
            /// Configuration Register B (Read/Write)
            /// </summary>
            ConfigurationRegisterB = 0x01,

            /// <summary>
            /// Mode Register (Read/Write)
            /// </summary>
            ModeRegister = 0x02,

            /// <summary>
            /// Data Output X MSB Register (Read only)
            /// </summary>
            DataOutputXMsbRegister = 0x03,

            /// <summary>
            /// Data Output X LSB Register (Read only)
            /// </summary>
            DataOutputXLsbRegister = 0x04,

            /// <summary>
            /// Data Output Z MSB Register (Read only)
            /// </summary>
            DataOutputZMsbRegister = 0x05,

            /// <summary>
            /// Data Output Z LSB Register (Read only)
            /// </summary>
            DataOutputZLsbRegister = 0x06,

            /// <summary>
            /// Data Output Y MSB Register (Read only)
            /// </summary>
            DataOutputYMsbRegister = 0x07,

            /// <summary>
            /// Data Output Y LSB Register (Read only)
            /// </summary>
            DataOutputYLsbRegister = 0x08,

            /// <summary>
            /// Status Register (Read only)
            /// </summary>
            StatusRegister = 0x09,

            /// <summary>
            /// Identification Register A (Read only)
            /// </summary>
            IdentificationRegisterA = 0x10,

            /// <summary>
            /// Identification Register B (Read only)
            /// </summary>
            IdentificationRegisterB = 0x11,

            /// <summary>
            /// Identification Register C (Read only)
            /// </summary>
            IdentificationRegisterC = 0x12
        }

        private enum SamplesAveraged : byte
        {

            /// <summary>
            /// 1 sample averaged per measurement output
            /// </summary>
            SamplesAveraged1 = 0x00,

            /// <summary>
            /// 2 samples averaged per measurement output
            /// </summary>
            SamplesAveraged2 = 0x20,

            /// <summary>
            /// 4 samples averaged per measurement output
            /// </summary>
            SamplesAveraged4 = 0x40,

            /// <summary>
            /// 8 samples averaged per measurement output
            /// </summary>
            SamplesAveraged8 = 0x60
        }

        private enum OutputRates : byte
        {
            /// <summary>
            /// 0.75 Hz Output Rate
            /// </summary>
            OUTPUT_RATE_0_75 = 0x00,

            /// <summary>
            /// 1.5 Hz Output Rate
            /// </summary>
            OUTPUT_RATE_1_5 = 0x04,

            /// <summary>
            /// 3 Hz Output Rate
            /// </summary>
            OUTPUT_RATE_3 = 0x08,

            /// <summary>
            /// 7.5 Hz Output Rate
            /// </summary>
            OUTPUT_RATE_7_5 = 0x0C,

            /// <summary>
            /// 15 Hz Output Rate
            /// </summary>
            OUTPUT_RATE_15 = 0x10,

            /// <summary>
            /// 30 Hz Output Rate
            /// </summary>
            OUTPUT_RATE_30 = 0x14,

            /// <summary>
            /// 75 Hz Output Rate
            /// </summary>
            OUTPUT_RATE_75 = 0x18
        }

        private enum Gain
        {
            /// <summary>
            /// Sensor Field Range ± 0.88 Ga (1370 LSb/Gauss)
            /// </summary>
            Gain1370 = 0x00,

            /// <summary>
            /// Sensor Field Range ± 1.3 Ga (1090 LSb/Gauss)
            /// </summary>
            Gain1090 = 0x20,

            /// <summary>
            /// Sensor Field Range ± 1.9 Ga (820 LSb/Gauss)
            /// </summary>
            Gain820 = 0x40,

            /// <summary>
            /// Sensor Field Range ± 2.5 Ga (660 LSb/Gauss)
            /// </summary>
            Gain660 = 0x60,

            /// <summary>
            /// Sensor Field Range ± 4.0 Ga (440 LSb/Gauss)
            /// </summary>
            GAIN_440 = 0x80,

            /// <summary>
            /// Sensor Field Range ± 4.7 Ga (390 LSb/Gauss)
            /// </summary>
            Gain390 = 0xA0,

            /// <summary>
            /// Sensor Field Range ± 5.6 Ga (330 LSb/Gauss)
            /// </summary>
            Gain330 = 0xC0,

            /// <summary>
            /// Sensor Field Range ± 8.1 Ga (230 LSb/Gauss)
            /// </summary>
            Gain230 = 0xE0
        }

        private enum MeasureModes : byte
        {
            /// <summary>
            /// Normal measurement configuration (Default).
            /// In normal measurement configuration the device follows normal measurement flow.
            /// The positive and negative pins of the resistive load are left floating and high impedance.
            /// </summary>
            MeasurementModeNormal = 0x00,

            /// <summary>
            /// Positive bias configuration for X, Y, and Z axes.
            /// In this configuration, a positive current is forced across the resistive load for all three axes.
            /// </summary>
            MeasurementModePositiveBias = 0x01,

            /// <summary>
            /// Negative bias configuration for X, Y and Z axes.
            /// In this configuration, a negative current is forced across the resistive load for all three axes.
            /// </summary>
            MeasurementModeNegativeBias = 0x01
        }

        public enum OperatingModes : byte
        {

            /// <summary>
            /// Continuous measurement mode
            /// </summary>
            OperatingModeContinuous = 0x00,

            /// <summary>
            /// Single measurement mode
            /// </summary>
            OperatingModeSingle = 0x01,

            /// <summary>
            /// Idle mode
            /// </summary>
            OperatingModeIdle = 0x02
        }

        public enum MeasurementModes
        {
            /// <summary>
            /// Magnetometer is in idle mode
            /// </summary>
            Idle = 0,

            /// <summary>
            /// Magnetometer is in continuous mode
            /// </summary>
            Continuous = 1,

            /// <summary>
            /// Magnetometer is in single measument mode
            /// </summary>
            Single = 2
        }

        /// <summary>
        /// Defines Pi
        /// </summary>
        //TODO: May NOT need to define if can find in system.Math...
        private const double Pi = 3.14159265358979323846;
        
        /// <summary>
        /// Modifier
        /// </summary>
        private float mgPerDigit = 0.92f;
        
        /// <summary>
        /// Class for the I²C connection
        /// </summary>
        private I2CDevice _i2CDevice;
        
        /// <summary>
        /// The variables for storing the results after the measurement.
        /// </summary>
        private byte[] i2CData = new byte[6];

        
        /// <summary>
        /// Struct to hold data
        /// </summary>
        public struct Data
        {
            /// <summary>
            /// Gets or sets the X axis.
            /// </summary>
            public double AxisX { get; set; }

            /// <summary>
            /// Gets or sets the Y axis.
            /// </summary>
            public double AxisY { get; set; }

            /// <summary>
            /// Gets or sets the Z axis.
            /// </summary>
            public double AxisZ { get; set; }

            /// <summary>
            /// Gets or sets the x offset.
            /// </summary>
            public double xOffset { get; set; }

            /// <summary>
            /// Gets or sets the y offset.
            /// </summary>
            public double yOffset { get; set; }
        }

        /// The constructor Initializes the connection and
        /// sets the sensor with a standard configuration.

        public Compass()
        {
            //Initialize all data to default values
            Data _data = new Data();
            _data.xOffset = 0.0;
            _data.yOffset = 0.0;
            _data.AxisX = 0.0;
            _data.AxisZ = 0.0;
            _data.AxisY = 0.0;

            // I²C bus address at 0x1E and clock speed of 100kHz
            _i2CDevice = new I2CDevice(new I2CDevice.Configuration((byte)Adresses.HMC588L_ADDRESS, 100));
            Thread.Sleep(100);

            // The configuration consists of two sections.
            // The first byte determines how many samples are taken per measurement
            // will be (Default = 1). In what bit rate to the outputs
            // is written (default = 15Hz) and the measuring mode is determined
            // the pretension (default = normal)
            // Default settings, see data sheet for further settings.
            StatusMessage(Write(new byte[] { (byte)Registers.ConfigurationRegisterA, 0x10 }));
            Thread.Sleep(100);

            // Standard scaling, see data sheet...
            // + - 1.3 Ga, 1090 Gain (LSb / Gauss)
            StatusMessage(Write(new byte[] { (byte)Registers.ConfigurationRegisterB, 0x20 }));
            Thread.Sleep(100);

            //setRange();
            //Thread.Sleep(100);
            // Operating Mode (0x02):
            // Continuous-Measurement Mode (0x00)
            //Write to the mode register (0x02) 
            StatusMessage(Write(new byte[] { (byte)Registers.ModeRegister, (byte)OperatingModes.OperatingModeContinuous}));
            Thread.Sleep(100);
            //StatusMessage(Write(new byte[] { (byte)Registers.HMC5883L_REG_CONFIG_A, (byte)hmc5883l_dataRate_t.HMC5883L_DATARATE_15HZ}));
            //Thread.Sleep(100);
            /*int value = Read(new byte[] { (byte)Registers.HMC5883L_REG_CONFIG_A});
            value &= 60;
            Convert.UseRFC4648Encoding.ToString();
            Convert.ToByte(valueStr);*/

        }

        /*public void setRange(hmc5883l_range_t range)
        {
            switch (range)
            {
                case hmc5883l_range_t.HMC5883L_RANGE_0_88GA:
                    mgPerDigit = 0.073f;
                    break;

                case hmc5883l_range_t.HMC5883L_RANGE_1_3GA:
                    mgPerDigit = 0.92f;
                    break;

                case hmc5883l_range_t.HMC5883L_RANGE_1_9GA:
                    mgPerDigit = 1.22f;
                    break;

                case hmc5883l_range_t.HMC5883L_RANGE_2_5GA:
                    mgPerDigit = 1.52f;
                    break;

                case hmc5883l_range_t.HMC5883L_RANGE_4GA:
                    mgPerDigit = 2.27f;
                    break;

                case hmc5883l_range_t.HMC5883L_RANGE_4_7GA:
                    mgPerDigit = 2.56f;
                    break;

                case hmc5883l_range_t.HMC5883L_RANGE_5_6GA:
                    mgPerDigit = 3.03f;
                    break;

                case hmc5883l_range_t.HMC5883L_RANGE_8_1GA:
                    mgPerDigit = 4.35f;
                    break;

                default:
                    break;
            }
            //Write((byte) Registers.HMC5883L_REG_CONFIG_B);//, int.Parse(range << 5));
            Write((byte[])range << 5);
        }*/


        public void setRange()
        {
            //hmc5883l_range_t.HMC5883L_RANGE_1_3GA;
            mgPerDigit = 0.92f;
            //Write((byte) Registers.HMC5883L_REG_CONFIG_B);//, int.Parse(range << 5));
            Write(new byte[] {(byte) Registers.ConfigurationRegisterB, (byte)Gain.Gain1090});
        }



        /// <summary>
        /// Reads the sensor measurements and writes them to the properties
        /// </summary>
        public void ReadRaw()
        {
            Data _data = new Data();
            // Sends the byte for the first axis
            Write(new byte[] { 0x03 });
            // Only if the reading was successful
            i2CData[0] = 0x03;
            if (Read(i2CData) != 0)
            {
                _data.AxisX = (i2CData[0] << 8) | i2CData[1];
                _data.AxisZ = (i2CData[2] << 8) | i2CData[3];
                _data.AxisY = (i2CData[4] << 8) | i2CData[5];
            }
            else
            {
                Debug.Print("Error reading compass!");
            }
            Debug.Print("X: "+ _data.AxisX+"   Y: "+ _data.AxisY+"   Z: "+ _data.AxisZ);
        }
        /// <summary>
        /// Updates heading from mag
        /// </summary>
        /// <returns></returns>
        public double ReadHeading()
        {
            Data _data = new Data();
            // Sends the byte for the first axis
            Write(new byte[] { 0x03 });
            
            // Only if the reading was successful
            i2CData[0] = 0x03;
            if (Read(i2CData) != 0)
            {
                _data.AxisX = (short)(((i2CData[0] << 8) | i2CData[1] )- _data.xOffset) * mgPerDigit;
                _data.AxisZ = (short)((i2CData[2] << 8) | i2CData[3]) * mgPerDigit;
                _data.AxisY = (short)(((i2CData[4] << 8) | i2CData[5]) - _data.yOffset) * mgPerDigit;
            }
            else
            {
                Debug.Print("Error reading compass!");
            }

            double heading = System.Math.Atan2(_data.AxisY, _data.AxisX);
            double declinationAngle = (4.0 + (26.0 / 60.0)) / (180 / Pi);

            heading += declinationAngle;

            if (heading < 0)
            {
                heading += 2 * Pi;
            }

            if (heading > 2 * Pi)
            {
                heading -= 2 * Pi;
            }

            double headingDegrees = heading * 180 / Pi;
            //double declinationAngle = 0.0404;
            //double headingDegrees;
            //if (AxisY > 0)
            //{
            //    headingDegrees = 90 - Math.Atan(AxisX / AxisY) * 180 / Pi;
            //}
            //else if (AxisY < 0)
            //{
            //    headingDegrees = 270 - Math.Atan(AxisX / AxisY) * 180 / Pi;
            //}
            //else if (AxisX < 0)
            //{
            //    headingDegrees = 180.0;
            //}
            //else
            //{
            //    headingDegrees = 0.0;
            //}
            //headingDegrees += declinationAngle;
            Debug.Print(headingDegrees.ToString());
            return headingDegrees;
        }

        public void SetOffset(int xo, int yo)
        {
            Data _data = new Data();
            _data.xOffset = xo;
            _data.yOffset = yo;
        }
        /// Sends the byte array to the module
        private int Write(byte[] buffer)
        {
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[]
            {
            I2CDevice.CreateWriteTransaction(buffer)
            };
            return _i2CDevice.Execute(transactions, 1000);
        }

        /// Reads the data from the module with the byte array
        private int Read(byte[] buffer)
        {
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] {0x03}),
                I2CDevice.CreateReadTransaction(buffer)
            };
            return _i2CDevice.Execute(transactions, 1000);
        }

        /// Returns the status of the operation
        /// about the output in Visual Studio.
        private void StatusMessage(int result)
        {
            if (result == 0)
            {
                Debug.Print("Status: Error while sending or receiving data to compass!");
            }
            else
            {
                Debug.Print("Status: OK");
            }
        }

        ~IrDistanceSensor()
        {
            Dispose();
        }
    }
}
