using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using System.Diagnostics;
using System.Threading;
//using LoveElectronics.Resources;
using Test_Bot_Multithread.Tools;
//using MicroFrameworkLibrary;
using Math = System.Math;

namespace Test_Bot_Multithread.Drivers
{

    public class HMC5883L : Wire
    {
        private const double declinationAngle = (8 + (12 / 60)) / (180 / Math.PI);
        private const byte HMC5883L_Address = 0x1E;
        private const byte ScaleRegister = 0x01;
        private const byte MeasurementRateRegister = 0x02;
        private const byte DataValue_StartingRegister = 0x03;
        private const byte IdentificationRegister_1 = 0x0A;
        private const byte IdentificationRegister_2 = 0x0B;
        private const byte IdentificationRegister_3 = 0x0C;

        private const byte IDENTIFICATION_REGISTER_A_VALUE = 0x48;
        private const byte IDENTIFICATION_REGISTER_B_VALUE = 0x34;
        private const byte IDENTIFICATION_REGISTER_C_VALUE = 0x33;
        private const byte CONTINUOUS_MEASUREMENT = 0x00;

        //private const int CLOCK_RATE = 100;
        public static int CLOCK_RATE = Program.clockSpeed;

        public HMC5883L(int timeout) : base(HMC5883L_Address, CLOCK_RATE, timeout)
        {
            //Connected();



            //SetScale(1.3);

            SetContinuous();
        }

        #region Abstract override methods

        public override bool Connected()
        {
            if ((DeviceIdentifier()[0] != (byte) IDENTIFICATION_REGISTER_A_VALUE) ||
                (DeviceIdentifier()[1] != (byte) IDENTIFICATION_REGISTER_B_VALUE) ||
                (DeviceIdentifier()[2] != (byte) IDENTIFICATION_REGISTER_C_VALUE))
            {
                throw new Exception("Did not return Device ID 0x48/0x34/0x33 as expected.");
            }
            return true;
        }


        public override byte[] DeviceIdentifier()
        {
            return new byte[3]
            {
                Read((byte) IdentificationRegister_1)[0],
                Read((byte) IdentificationRegister_2)[0],
                Read((byte) IdentificationRegister_3)[0]
            };
        }

        #endregion

        public int readRaw()
        {
            Debug.Print("X = " + Raw.X + " Y = " + Raw.Y + "Z = " + Raw.Z);
            return 1;
        }

        //public short ReadHeading()
        //{
        //    double heading = Math.Atan2(ScaledData.ScaledY, ScaledData.ScaledX);

        //    heading += Math.PI / 2;
        //    if (heading < 0)
        //        heading += 2 * Math.PI;
        //    if (heading > 2 * Math.PI)
        //        heading -= 2 * Math.PI;

        //    short headingDegrees = (short)(heading * 180 / Math.PI);

        //    return headingDegrees;

        //}

        private double pitch, roll;
        private double declination; // in milli radians

        // If you have an EAST declination, use positive value, if you have a WEST declination, use negative value
        public void SetDeclination(double dec_mRads)
        {
            declination = dec_mRads / 1000.0;
        }

        public void SetPitch(double p)
        {
            pitch = p;
        }

        public double GetPitch()
        {
            return pitch;
        }

        public int GetPitchDegrees()
        {
            return (int) ExMath.RadiansToDegrees(pitch);
        }

        public void SetRoll(double r)
        {
            roll = r;
        }

        public double GetRoll()
        {
            return roll;
        }

        public double GetRollDegrees()
        {
            return (int) ExMath.RadiansToDegrees(roll);
        }

        public struct ThreeAxisRawData
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        public struct ThreeAxisScaledData
        {
            public double ScaledX { get; set; }
            public double ScaledY { get; set; }
            public double ScaledZ { get; set; }
        }


        #region Three Axis Operations

        public ThreeAxisRawData Raw
        {
            get
            {
                var r = new ThreeAxisRawData();

                var bytes = this.Read((byte) 0x03, 6);

                short xReading = (short) ((bytes[0] << 8) | bytes[1]);
                short zReading = (short) ((bytes[2] << 8) | bytes[3]);
                short yReading = (short) ((bytes[4] << 8) | bytes[5]);

                r.X = xReading;
                r.Y = yReading;
                r.Z = zReading;

                return r;
            }
        }

        public ThreeAxisScaledData ScaledData
        {
            get
            {
                ThreeAxisRawData rd = this.Raw; // We will read data only once
                var s = new ThreeAxisScaledData();

                s.ScaledX = Scale * rd.X;
                s.ScaledY = Scale * rd.Y;
                s.ScaledZ = Scale * rd.Z;

                return s;
            }
        }

        #endregion

        public double GetHeadingRaw()
        {
            ThreeAxisScaledData sd = this.ScaledData; // this ensures we are reading data only once from the bus

            return ExMath.Atan(sd.ScaledY / sd.ScaledX);
        }

        public static double Scale { get; set; }

        public double GetHeadingRawDegrees()
        {
            return ExMath.RadiansToDegrees(this.GetHeadingRaw());
        }

        public double GetHeadingDegrees()
        {
            return ExMath.RadiansToDegrees(this.GetHeading());
        }


        public double GetHeading()
        {
            ThreeAxisScaledData sd;
            double Xm, Ym, Zm;
            double Xh, Yh;
            double cp, sp, cr, sr; // Cos and Sin of Pitch and Roll
            double result;

            sd = this.ScaledData; // this ensures we are only reading once from the bus
            Xm = sd.ScaledX; // swapping X=Y due to installment 
            Ym = sd.ScaledY;
            Zm = sd.ScaledZ;

            cp = ExMath.Cos(pitch);
            sp = ExMath.Sin(pitch);
            cr = ExMath.Cos(roll);
            sr = ExMath.Sin(roll);
#if false
// Below algorithm for tilt taken from LoveElectronics and proved to be wrong:
// Roll causes heading to swing and be incorrect
            Xh = Xm * cp + Zm * sp;
            Yh = Xm * sr * sp + Ym * cr - Zm * sr * cp;
            result = ExMath.Atan(Yh / Xh);
#else
            // Below algorthing for tilt compensation taken from
            // http://www51.honeywell.com/aero/common/documents/myaerospacecatalog-documents/Defense_Brochures-documents/Magnetic__Literature_Technical_Article-documents/Applications_of_Magnetic_Sensors_for_Low_Cost_Compass_Systems.pdf
            // Goto http://www.magneticsensors.com choose Literature and on the "Technical Articles" select the above article
            // results are great!
            Xh = Xm * cp + Ym * sr * sp - Zm * cr * sp;
            Yh = Ym * cr + Zm * sr;
            result = ExMath.Atan(Yh / Xh);
            if (Xh < 0)
                result = ExMath.PI - result;
            else if (Xh > 0)
                if (Yh < 0)
                    result = -result;
                else if (Yh > 0)
                    result = 2 * ExMath.PI - result;
                else ;
            else // Xh == 0
            if (Yh < 0)
                result = ExMath.PI / 2; // 90 degrees
            else
                result = ExMath.PI + ExMath.PI / 2; // 270 degrees         
#endif

            // Add the declination value
            result += declination;
            return result;
        }


        //public override bool Connected()
        //{
        //    if (DeviceIdentifier()[0] != IDENTIFICATION_REGISTER_A_VALUE
        //        || DeviceIdentifier()[1] != IDENTIFICATION_REGISTER_B_VALUE
        //        || DeviceIdentifier()[2] != IDENTIFICATION_REGISTER_C_VALUE)
        //    {
        //        throw new Exception("Did not return Device ID 0x48/0x34/0x33 as expected.");
        //    }
        //    return true;
        //}

        //public override byte[] DeviceIdentifier()
        //{
        //    return new byte[3] { Read(IdentificationRegister_1)[0], Read(IdentificationRegister_2)[0], Read(IdentificationRegister_3)[0] };
        //}

        public void SetContinuous()
        {
            this.Write(MeasurementRateRegister, CONTINUOUS_MEASUREMENT);
        }

        public void SetScale(double gauss)
        {
            byte regValue = 0x00;

            if (gauss == 0.88)
            {
                regValue = 0x00;
                Scale = 0.73;
            }
            else if (gauss == 1.3)
            {
                regValue = 0x01;
                Scale = 0.92;
            }
            else if (gauss == 1.9)
            {
                regValue = 0x02;
                Scale = 1.22;
            }
            else if (gauss == 2.5)
            {
                regValue = 0x03;
                Scale = 1.52;
            }
            else if (gauss == 4.0)
            {
                regValue = 0x04;
                Scale = 2.27;
            }
            else if (gauss == 4.7)
            {
                regValue = 0x05;
                Scale = 2.56;
            }
            else if (gauss == 5.6)
            {
                regValue = 0x06;
                Scale = 3.03;
            }
            else if (gauss == 8.1)
            {
                regValue = 0x07;
                Scale = 4.35;
            }
            else
            {
                throw new ArgumentException("Unknown gauss value");
            }

            regValue = (byte) (((int) regValue) << 5);

            this.Write(ScaleRegister, regValue);
        }
    }
}

/*
public RawData Raw
{
    get
    {
        var r = new RawData();

        var bytes = this.Read(DataValue_StartingRegister, 6);

        short xReading = (short)((bytes[0] << 8) | bytes[1]);
        short zReading = (short)((bytes[2] << 8) | bytes[3]);
        short yReading = (short)((bytes[4] << 8) | bytes[5]);

        r.X = xReading;
        r.Y = yReading;
        r.Z = zReading;

        return r;
    }
}

public ScaledData ScaledData
{
    get
    {
        var s = new ScaledData();
        s.ScaledX = Scale * (double)this.Raw.X;
        s.ScaledY = Scale * (double)this.Raw.Y;
        s.ScaledZ = Scale * (double)this.Raw.Z;

        return s;
    }
}
}

public struct RawData
{
public int X { get; set; }
public int Y { get; set; }
public int Z { get; set; }
}

public struct ScaledData
{
public double ScaledX { get; set; }
public double ScaledY { get; set; }
public double ScaledZ { get; set; }
}
}
*/



    //public class Compass
    //{
       














    ////#region NewCode
    ////private readonly I2CDevice.Configuration _slaveConfig;
    ////private const int TransactionTimeout = 1000; // ms

    ////public Compass(byte address = 0x1E, OperatingModes mode = OperatingModes.OperatingModeContinuous)
    ////{
    ////    HMC5883LAdress = address;
    ////    _slaveConfig = new I2CDevice.Configuration(HMC5883LAdress, 100);
    ////    //bug: Compass is not acknowledging byte write
    ////    //I2CBus.GetInstance().WriteRegister(_slaveConfig,(byte)Registers.ConfigurationRegisterA, (byte) SamplesAveraged.SamplesAveraged8, TransactionTimeout);
    ////    //I2CBus.GetInstance().WriteRegister(_slaveConfig, new byte[] {0x3C, (byte)Registers.ConfigurationRegisterA, (byte)SamplesAveraged.SamplesAveraged8}, TransactionTimeout);
    ////    //Thread.Sleep(50);
    ////    //I2CBus.GetInstance().WriteRegister(_slaveConfig,(byte)Registers.ConfigurationRegisterB, (byte)Gain.Gain1090, TransactionTimeout);
    ////    //Thread.Sleep(50);
    ////    //I2CBus.GetInstance().WriteRegister(_slaveConfig, (byte)Registers.ModeRegister, (byte)OperatingModes.OperatingModeContinuous, TransactionTimeout);
    ////    //I2CBus.GetInstance().Write(_slaveConfig, new byte[] { 0x3C, 0x02, 0x00}, TransactionTimeout);
    ////    //Thread.Sleep(6);
    ////    //while (!Init(mode))
    ////        Debug.Print("HMC3885L magnatometer not detected...");
    ////}

    //////public bool Init(OperatingModes mode = OperatingModes.OperatingModeContinuous)
    //////{
    //////    if (mode > OperatingModes.OperatingModeContinuous || mode < 0)
    //////        _mode = OperatingModes.OperatingModeContinuous;
    //////    else
    //////        _mode = mode;


    //////    byte[] whatMode = { 0 };

    //////    I2CBus.GetInstance().ReadRegister(_slaveConfig, (byte)Registers.ModeRegister, whatMode, TransactionTimeout);

    //////    if (whatMode[0] != (byte)OperatingModes.OperatingModeContinuous) return false;

    //////    //ReadCoefficients();

    //////    return true;
    //////}


    ////public short readHeading()
    ////{
    ////    //var readBuffer = new byte[6];
    ////    //var data = new Data();
    ////    //I2CBus.GetInstance().ReadRegister(_slaveConfig, 0x03, readBuffer, TransactionTimeout);

    ////    //I2CBus.GetInstance().Read(_slaveConfig, readBuffer, TransactionTimeout);
    ////    //data.AxisX = (readBuffer[0] << 8) | readBuffer[1];
    ////    //data.AxisZ = (readBuffer[2] << 8) | readBuffer[3];
    ////    //data.AxisY = (readBuffer[4] << 8) | readBuffer[5];

    ////    Data data = new Data();
    ////    var buffer = new byte[6];
    ////    buffer[0] = 0x03;
    ////    I2CBus.GetInstance().Write(_slaveConfig,new byte[] { 0x3D, 0x06}, TransactionTimeout);
    ////    //I2CBus.GetInstance().Read(new I2CDevice.Configuration(0x1E, 100), buffer, TransactionTimeout);
    ////    //I2CBus.GetInstance().Write(_slaveConfig, new byte[] {0x3C, 0x03 }, TransactionTimeout);
    ////    I2CBus.GetInstance().ReadRegister(_slaveConfig, 0x03, buffer, TransactionTimeout);

    ////    I2CBus.GetInstance().Write(_slaveConfig, new byte[] {0x3C, 0x03 }, TransactionTimeout);

    ////    data.AxisX = ((buffer[0] << 8) | buffer[1]);
    ////    data.AxisZ = ((buffer[2] << 8) | buffer[3]);
    ////    data.AxisY = ((buffer[4] << 8) | buffer[5]);

    ////    Debug.Print("x: "+data.AxisX+" Y: "+ data.AxisY+ " Z: "+ data.AxisZ);
    ////    return 1234;
    ////}

    ////#endregion



    //#region Definitions
    ///// <summary>
    ///// Defines Pi
    ///// </summary>
    ////TODO: May NOT need to define if can find in system.Math...
    //private const double Pi = 3.14159265358979323846;

    //    /// <summary>
    //    /// Modifier
    //    /// </summary>
    //    private float _mgPerDigit = 0.92f;

    //    /// <summary>
    //    /// Class for the I²C connection
    //    /// </summary>
    //    private I2CDevice _i2CDevice;

    //    /// <summary>
    //    /// The variables for storing the results after the measurement.
    //    /// </summary>
    //    private byte[] i2CData = new byte[6];


    //    /// <summary>
    //    /// Struct to hold data
    //    /// </summary>
    //    public struct Data
    //    {
    //        /// <summary>
    //        /// Gets or sets the X axis.
    //        /// </summary>
    //        public double AxisX { get; set; }

    //        /// <summary>
    //        /// Gets or sets the Y axis.
    //        /// </summary>
    //        public double AxisY { get; set; }

    //        /// <summary>
    //        /// Gets or sets the Z axis.
    //        /// </summary>
    //        public double AxisZ { get; set; }

    //        /// <summary>
    //        /// Gets or sets the x offset.
    //        /// </summary>
    //        public short xOffset { get; set; }

    //        /// <summary>
    //        /// Gets or sets the y offset.
    //        /// </summary>
    //        public short yOffset { get; set; }

    //        /// <summary>
    //        /// Gets or sets the y offset.
    //        /// </summary>
    //        public short zOffset { get; set; }

    //        /// <summary>
    //        /// Gets or sets the y offset.
    //        /// </summary>
    //        public double xScale { get; set; }

    //        /// <summary>
    //        /// Gets or sets the y offset.
    //        /// </summary>
    //        public double yScale { get; set; }

    //        /// <summary>
    //        /// Gets or sets the y offset.
    //        /// </summary>
    //        public double zScale { get; set; }

    //        public double declinationAngle { get; set; }
    //    }
    //    #endregion

    ////    #region OldCode

    ////    /// The constructor Initializes the connection and
    ////    /// sets the sensor with a standard configuration.

    ////    //public Compass()
    ////    //{
    ////    //    //Initialize all data to default values
    ////    //    Data data = new Data();
    ////    //    data.AxisX = 0;
    ////    //    data.AxisZ = 0;
    ////    //    data.AxisY = 0;
    ////    //    data.xOffset = -7;
    ////    //    data.yOffset = 54;
    ////    //    data.zOffset = 0;
    ////    //    data.xScale = 1.070;
    ////    //    data.yScale = 1.117;
    ////    //    data.zScale = 1;
    ////    //    // Formula: (deg + (min / 60.0)) / (180 / PI);
    ////    //    data.declinationAngle = (8 + (12 / 60)) / (180 / Pi);

    ////    //    // I2C bus address at 0x1E and clock speed of 100kHz
    ////    //    _i2CDevice = new I2CDevice(new I2CDevice.Configuration((byte)Adresses.HMC588L_ADDRESS, 100));
    ////    //    Thread.Sleep(100);


    ////    //    // Operating Mode (0x02):
    ////    //    // Continuous-Measurement Mode (0x00)
    ////    //    //Write to the mode register (0x02) 
    ////    //    StatusMessage(Write(new byte[] { (byte)Registers.ModeRegister, (byte)OperatingModes.OperatingModeContinuous }));
    ////    //    Thread.Sleep(100);


    ////    //    // Standard scaling, see data sheet...
    ////    //    // + - 1.3 Ga, 1090 Gain (LSb / Gauss)
    ////    //    //StatusMessage(Write(new byte[] {(byte) Registers.ConfigurationRegisterB, 0x20}));
    ////    //    StatusMessage(Write(new byte[] { (byte)Registers.ConfigurationRegisterB, (byte)Gain.Gain1090 }));
    ////    //    Thread.Sleep(100);


    ////    //    // The configuration consists of two sections.
    ////    //    // The first byte determines how many samples are taken per measurement
    ////    //    // will be (Default = 1). In what bit rate to the outputs
    ////    //    // is written (default = 15Hz) and the measuring mode is determined
    ////    //    // the pretension (default = normal)
    ////    //    // Default settings, see data sheet for further settings.
    ////    //    //StatusMessage(Write(new byte[] { (byte)Registers.ConfigurationRegisterA, 0x10 }));
    ////    //    StatusMessage(Write(new byte[] { (byte)Registers.ConfigurationRegisterA, (byte)OutputRates.OUTPUT_RATE_30 }));
    ////    //    Thread.Sleep(100);



    ////    //    StatusMessage(Write(new byte[] { (byte)Registers.ConfigurationRegisterA, (byte)SamplesAveraged.SamplesAveraged8 }));
    ////    //    Thread.Sleep(100);


    ////    //}


        

    ////    /*
    ////    /// <summary>
    ////    /// Reads the sensor measurements and writes them to the properties
    ////    /// </summary>
    ////    public double ReadRaw()
    ////    {
    ////        Data data = new Data();
    ////        //byte[6] i2CData[] = new byte[6];
    ////        // Sends the byte for the first axis
    ////        Write(new byte[] { 0x03 });
    ////        Thread.Sleep(10);
    ////        i2CData[0] = 0x03;
    ////        Read(i2CData);
    ////        // Only if the reading was successful
    ////        //i2CData[0] = 0x03;
    ////        //if (Read(i2CData) != 0)
    ////        if (i2CData[0] != 0)
    ////        {
    ////            data.AxisX = ((i2CData[0] << 8) | i2CData[1]);
    ////            data.AxisZ = ((i2CData[2] << 8) | i2CData[3]);
    ////            data.AxisY = ((i2CData[4] << 8) | i2CData[5]);
    ////        }
    ////        else
    ////        {
    ////            Debug.Print("Error reading compass!");
    ////        }
    ////        Debug.Print("X: " + data.AxisX + "   Y: " + data.AxisY + "   Z: " + data.AxisZ);
    ////        return data.AxisY;
    ////    }

    ////    /// <summary>
    ////    /// Returns heading in degrees from mag
    ////    /// </summary>
    ////    /// <returns></returns>
    ////    public short ReadHeading()
    ////    {
    ////        Data data = new Data();
    ////        // Sends the byte for the first axis
    ////        Write(new byte[] { 0x03 });
    ////        Read(i2CData);
    ////        // Only if the reading was successful
    ////        //i2CData[0] = 0x03;
    ////        if (Read(i2CData) != 0)
    ////        {
    ////            //data.AxisX = (int) (((i2CData[0] << 8) | i2CData[1]) - data.xOffset) * _mgPerDigit;
    ////            //data.AxisZ = (double) ((i2CData[2] << 8) | i2CData[3]) * _mgPerDigit;
    ////            //data.AxisY = (int) (((i2CData[4] << 8) | i2CData[5]) - data.yOffset) * _mgPerDigit;
    ////            data.AxisX = ((i2CData[0] << 8) | i2CData[1]);
    ////            data.AxisZ = ((i2CData[2] << 8) | i2CData[3]);
    ////            data.AxisY = ((i2CData[4] << 8) | i2CData[5]);
    ////        }
    ////        else
    ////        {
    ////            Debug.Print("Error reading compass!");
    ////        }

    ////        /*
    ////        //Adjust values by offsets
    ////        data.AxisX += data.xOffset;
    ////        data.AxisY += data.yOffset;
    ////        data.AxisZ += data.zOffset;

    ////        //Aplly scalers to axes
    ////        data.AxisX *= data.xScale;
    ////        data.AxisY *= data.yScale;
    ////        data.AxisZ *= data.zScale;
    ////        */
    ////    /*
    ////    double heading = Math.Atan2(data.AxisY, data.AxisX);
    ////    //return heading;

    ////    //heading += Pi / 2;
    ////    //if (heading < 0) heading += 2 * Pi;
    ////    //if (heading > 2 * Pi) heading -= 2 * Pi;

    ////    //if (heading < 0) heading += 2*Pi;
    ////    //heading = 360 - heading;

    ////    //double declinationAngle = (4.0 + (26.0 / 60.0)) / (180 / Pi);

    ////    heading += data.declinationAngle;

    ////    if (heading < 0)
    ////    {
    ////        heading += 2 * Pi;
    ////    }

    ////    if (heading > 2 * Pi)
    ////    {
    ////        heading -= 2 * Pi;
    ////    }

    ////    heading = heading * 180 / Pi;
    ////    short headingDegrees = (short)heading;
    ////    //heading = heading * 180 / Pi;
    ////    //ushort headingDegrees = (ushort)heading;
    ////    //Debug.Print(headingDegrees.ToString());

    ////    return headingDegrees;
    ////}

    ////public void SetOffset(short xo, short yo)
    ////{
    ////    Data data = new Data();
    ////    data.xOffset = xo;
    ////    data.yOffset = yo;
    ////}

    /////// Sends the byte array to the module
    ////private int Write(byte[] buffer)
    ////{
    ////    I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[]
    ////    {
    ////        I2CDevice.CreateWriteTransaction(buffer)
    ////    };
    ////    return _i2CDevice.Execute(transactions, 1000);
    ////}

    /////// Reads the data from the module with the byte array
    ////private int Read(byte[] buffer)
    ////{
    ////    /*I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[]
    ////    {
    ////        I2CDevice.CreateWriteTransaction(new byte[] {0x03}),
    ////        I2CDevice.CreateReadTransaction(buffer)
    ////    };*/
    ////    /*
    ////    I2CDevice.I2CTransaction[] writeTransaction = new I2CDevice.I2CTransaction[]
    ////    {
    ////        I2CDevice.CreateWriteTransaction(new byte[] {0x03})
    ////    };
    ////    _i2CDevice.Execute(writeTransaction, 1000);

    ////    Thread.Sleep(100);

    ////    I2CDevice.I2CTransaction[] readTransacion = new I2CDevice.I2CTransaction[] {
    ////        I2CDevice.CreateReadTransaction(buffer)
    ////    };
    ////    _i2CDevice.Execute(readTransacion, 1000);
    ////    //return _i2CDevice.Execute(transactions, 1000);
    ////    return 0;
    ////}


    ////private int ReadWrite(byte[] readBuffer, byte writeBuffer)
    ////{
    ////    I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[]
    ////    {
    ////        I2CDevice.CreateWriteTransaction(new byte[] {writeBuffer}),
    ////        I2CDevice.CreateReadTransaction(readBuffer)
    ////    };
    ////    return _i2CDevice.Execute(transactions, 1000);
    ////}



    /////// Returns the status of the operation
    /////// about the output in Visual Studio.
    ////private void StatusMessage(int result)
    ////{
    ////    if (result == 0)
    ////    {
    ////        Debug.Print("Status: Error while sending or receiving data to compass!");
    ////    }
    ////    else
    ////    {
    ////        Debug.Print("Status: OK");
    ////    }
    ////}*/
    ////    #endregion

    //    #region Stuff
    //    private enum Adresses : byte
    //    {

    //        /// <summary>
    //        /// 7-bit I2C physical address of the HMC5883L
    //        /// </summary>
    //        HMC588L_ADDRESS = 0x1E,

    //        /// <summary>
    //        /// 7-bit I2C read address of the HMC5883L
    //        /// </summary>
    //        HMC588L_ADDRESS_READ = 0x3D,

    //        /// <summary>
    //        /// 7-bit I2C write address of the HMC5883L
    //        /// </summary>
    //        HMC588L_ADDRESS_WRITE = 0x3C
    //    }

    //    private enum Registers : byte
    //    {
    //        /// <summary>
    //        /// Configuration Register A (Read/Write)
    //        /// </summary>
    //        ConfigurationRegisterA = 0x00,

    //        /// <summary>
    //        /// Configuration Register B (Read/Write)
    //        /// </summary>
    //        ConfigurationRegisterB = 0x01,

    //        /// <summary>
    //        /// Mode Register (Read/Write)
    //        /// </summary>
    //        ModeRegister = 0x02,

    //        /// <summary>
    //        /// Data Output X MSB Register (Read only)
    //        /// </summary>
    //        DataOutputXMsbRegister = 0x03,

    //        /// <summary>
    //        /// Data Output X LSB Register (Read only)
    //        /// </summary>
    //        DataOutputXLsbRegister = 0x04,

    //        /// <summary>
    //        /// Data Output Z MSB Register (Read only)
    //        /// </summary>
    //        DataOutputZMsbRegister = 0x05,

    //        /// <summary>
    //        /// Data Output Z LSB Register (Read only)
    //        /// </summary>
    //        DataOutputZLsbRegister = 0x06,

    //        /// <summary>
    //        /// Data Output Y MSB Register (Read only)
    //        /// </summary>
    //        DataOutputYMsbRegister = 0x07,

    //        /// <summary>
    //        /// Data Output Y LSB Register (Read only)
    //        /// </summary>
    //        DataOutputYLsbRegister = 0x08,

    //        /// <summary>
    //        /// Status Register (Read only)
    //        /// </summary>
    //        StatusRegister = 0x09,

    //        /// <summary>
    //        /// Identification Register A (Read only)
    //        /// </summary>
    //        IdentificationRegisterA = 0x10,

    //        /// <summary>
    //        /// Identification Register B (Read only)
    //        /// </summary>
    //        IdentificationRegisterB = 0x11,

    //        /// <summary>
    //        /// Identification Register C (Read only)
    //        /// </summary>
    //        IdentificationRegisterC = 0x12
    //    }

    //    private enum SamplesAveraged : byte
    //    {

    //        /// <summary>
    //        /// 1 sample averaged per measurement output
    //        /// </summary>
    //        SamplesAveraged1 = 0x00,

    //        /// <summary>
    //        /// 2 samples averaged per measurement output
    //        /// </summary>
    //        SamplesAveraged2 = 0x20,

    //        /// <summary>
    //        /// 4 samples averaged per measurement output
    //        /// </summary>
    //        SamplesAveraged4 = 0x40,

    //        /// <summary>
    //        /// 8 samples averaged per measurement output
    //        /// </summary>
    //        SamplesAveraged8 = 0x70
    //    }

    //    private enum OutputRates : byte
    //    {
    //        /// <summary>
    //        /// 0.75 Hz Output Rate
    //        /// </summary>
    //        OUTPUT_RATE_0_75 = 0x00,

    //        /// <summary>
    //        /// 1.5 Hz Output Rate
    //        /// </summary>
    //        OUTPUT_RATE_1_5 = 0x04,

    //        /// <summary>
    //        /// 3 Hz Output Rate
    //        /// </summary>
    //        OUTPUT_RATE_3 = 0x08,

    //        /// <summary>
    //        /// 7.5 Hz Output Rate
    //        /// </summary>
    //        OUTPUT_RATE_7_5 = 0x0C,

    //        /// <summary>
    //        /// 15 Hz Output Rate
    //        /// </summary>
    //        OUTPUT_RATE_15 = 0x10,

    //        /// <summary>
    //        /// 30 Hz Output Rate
    //        /// </summary>
    //        OUTPUT_RATE_30 = 0x14,

    //        /// <summary>
    //        /// 75 Hz Output Rate
    //        /// </summary>
    //        OUTPUT_RATE_75 = 0x18
    //    }

    //    private enum Gain
    //    {
    //        /// <summary>
    //        /// Sensor Field Range ± 0.88 Ga (1370 LSb/Gauss)
    //        /// </summary>
    //        Gain1370 = 0x00,

    //        /// <summary>
    //        /// Sensor Field Range ± 1.3 Ga (1090 LSb/Gauss)
    //        /// </summary>
    //        Gain1090 = 0x20,

    //        /// <summary>
    //        /// Sensor Field Range ± 1.9 Ga (820 LSb/Gauss)
    //        /// </summary>
    //        Gain820 = 0x40,

    //        /// <summary>
    //        /// Sensor Field Range ± 2.5 Ga (660 LSb/Gauss)
    //        /// </summary>
    //        Gain660 = 0x60,

    //        /// <summary>
    //        /// Sensor Field Range ± 4.0 Ga (440 LSb/Gauss)
    //        /// </summary>
    //        GAIN_440 = 0x80,

    //        /// <summary>
    //        /// Sensor Field Range ± 4.7 Ga (390 LSb/Gauss)
    //        /// </summary>
    //        Gain390 = 0xA0,

    //        /// <summary>
    //        /// Sensor Field Range ± 5.6 Ga (330 LSb/Gauss)
    //        /// </summary>
    //        Gain330 = 0xC0,

    //        /// <summary>
    //        /// Sensor Field Range ± 8.1 Ga (230 LSb/Gauss)
    //        /// </summary>
    //        Gain230 = 0xE0
    //    }

    //    private enum MeasureModes : byte
    //    {
    //        /// <summary>
    //        /// Normal measurement configuration (Default).
    //        /// In normal measurement configuration the device follows normal measurement flow.
    //        /// The positive and negative pins of the resistive load are left floating and high impedance.
    //        /// </summary>
    //        MeasurementModeNormal = 0x00,

    //        /// <summary>
    //        /// Positive bias configuration for X, Y, and Z axes.
    //        /// In this configuration, a positive current is forced across the resistive load for all three axes.
    //        /// </summary>
    //        MeasurementModePositiveBias = 0x01,

    //        /// <summary>
    //        /// Negative bias configuration for X, Y and Z axes.
    //        /// In this configuration, a negative current is forced across the resistive load for all three axes.
    //        /// </summary>
    //        MeasurementModeNegativeBias = 0x01
    //    }

    //    public enum OperatingModes : byte
    //    {

    //        /// <summary>
    //        /// Continuous measurement mode
    //        /// </summary>
    //        OperatingModeContinuous = 0x00,

    //        /// <summary>
    //        /// Single measurement mode
    //        /// </summary>
    //        OperatingModeSingle = 0x01,

    //        /// <summary>
    //        /// Idle mode
    //        /// </summary>
    //        OperatingModeIdle = 0x02
    //    }

    //    public enum MeasurementModes
    //    {
    //        /// <summary>
    //        /// Magnetometer is in idle mode
    //        /// </summary>
    //        Idle = 0,

    //        /// <summary>
    //        /// Magnetometer is in continuous mode
    //        /// </summary>
    //        Continuous = 1,

    //        /// <summary>
    //        /// Magnetometer is in single measument mode
    //        /// </summary>
    //        Single = 2
    //    }
    //    #endregion
    //}
//}
