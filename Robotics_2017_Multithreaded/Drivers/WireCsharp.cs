/* ===================================================================================
 * Classes to help using multiple devices on I2C bus
 * ===================================================================================
 * Inspired by a sample posted on http://forums.netduino.com (Netduino Forums) by
 * Jeremy (http://forums.netduino.com/index.php?/topic/2545-hmc5883l-magnetometer-netduino-code/)
 * And modified to fit indeed multiple devices on the I2C bus by by Zakie Mashiah
 * ===================================================================================
 * You may use, copy, borrow, modify or do anything you like with this code. Crediting
 * the author will be appreciated.
 * ===================================================================================
 */
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Test_Bot_Multithread
{

    /// <summary>
    /// This class is used to have a single bus interface on the system as typically is the case on microprocessors.
    /// For some reason Microsoft chose to call the bus I2Device which is wrong as that class represent the bus really
    /// and not a single device on it.
    /// For simplicity we hold one I2Device.Configuration that will help programmers build classes to have interface 
    /// with single device on the bus with no hassle. If only a single device exist on the bus then program will be
    /// calling the 'Execute' function without having the configuration sent on every call.
    /// </summary>
    public static class I2CBus
    {
        public enum I2CBusSpeed : int
        {
            ClockRate100 = 100,
            ClockRate400 = 400
        }

        static I2CDevice.Configuration currentConfig;
        static I2CDevice theBus;

        public static void SetConfig(ushort address, int clockRate)
        {
            currentConfig = new I2CDevice.Configuration(address, clockRate);
            if (theBus == null) // good time to initialize the bus
                theBus = new I2CDevice(currentConfig);
        }

        public static void SetConfig(I2CDevice.Configuration config)
        {
            currentConfig = config;
            if (theBus == null) // good time to initialize the bus
                theBus = new I2CDevice(currentConfig);
        }

        public static int Execute(I2CDevice.I2CTransaction[] xAction, int timeout)
        {
            theBus.Config = currentConfig;
            return theBus.Execute(xAction, timeout);
        }

        public static int Execute(I2CDevice.Configuration config, I2CDevice.I2CTransaction[] xAction, int timeout)
        {
            theBus.Config = config;
            return theBus.Execute(xAction, timeout);
        }
    }



    /// <summary>
    /// This class helps abstract multiple devices on the I2C bus, so that every class representing a device should inherit this class
    /// and just implement the two abstract methods 'Connected' and 'DeviceIdentifiier' which are failry straight forward on most devices
    /// Also this class offers the option to read 16 bit values (short) and not only bytes array.
    /// </summary>
    public abstract class Wire
    {
        I2CDevice.Configuration myConfig;
        int Timeout;

        public Wire(ushort address, int clockRate, int timeout) // : base(new Configuration(address, clockRate)) 
        {
            myConfig = new I2CDevice.Configuration(address, clockRate);
            Timeout = timeout;
            I2CBus.SetConfig(myConfig);
        }


        public byte[] Read(byte addressToReadFrom, int responseLength = 1)
        {
            var buffer = new byte[responseLength];
            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] { addressToReadFrom }),
                I2CDevice.CreateReadTransaction(buffer)
            };
            int result = I2CBus.Execute(myConfig, transaction, Timeout);
            return buffer;
        }


        /// <summary>
        /// Reads 16 bit value from two registers on the I2C device
        /// </summary>
        /// <param name="addrMSB"></param>
        /// <param name="addrLSB"></param>
        /// <returns></returns>
        public short ReadShort(byte addrMSB, byte addrLSB)
        {
            short result;
            byte startAddr = 0;
            bool highFirst = false;
            byte[] data;

            // See if the addresses are continous and what order
            if ((addrLSB + 1) == addrMSB)
            {
                startAddr = addrLSB;
                highFirst = false;
            }
            else
                if ((addrMSB + 1) == addrLSB)
            {
                startAddr = addrMSB;
                highFirst = true;
            }

            // If they are continous then read 2 bytes from the bus
            if (startAddr != 0)
            {
                data = Read(startAddr, 2);

                if (highFirst)
                    result = (Int16)(data[0] << 8 | data[1]);
                else
                    result = (Int16)(data[1] << 8 | data[0]);
            }
            else
            {
                // Read one byte at a time
                byte lowV, highV;

                lowV = Read(addrLSB)[0];
                highV = Read(addrMSB)[0];
                result = (Int16)(highV << 8 | lowV);
            }

            return result;
        }



        public void Write(byte addressToWriteTo, byte valueToWrite)
        {
            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] { addressToWriteTo, valueToWrite })
            };
            int result = I2CBus.Execute(myConfig, transaction, Timeout);
        }

        public abstract bool Connected();

        public abstract byte[] DeviceIdentifier();
    }

}















//#region Old Code


//namespace MicroFrameworkLibrary
//{
//    using System;
//    using Microsoft.SPOT;
//    using Microsoft.SPOT.Hardware;



//    public abstract class WireCsharp : I2CDevice, IDisposable
//    {
//        public WireCsharp(ushort address, int clockRate, int timeout) : base(new Configuration(address, clockRate)) 
//        {
//            Address = address;
//            ClockRate = clockRate;
//            Timeout = timeout;
//        } 

//        public static ushort Address { get; set; }

//        public static int ClockRate { get; set; }

//        public static int Timeout { get; set; }

//        public static I2CDevice GetInstance()
//        {
//            return new I2CDevice(new I2CDevice.Configuration(Address, ClockRate));
//        }

//        public static I2CDevice GetInstance(ushort address, int clockRate)
//        {
//            return new I2CDevice(new I2CDevice.Configuration(address, clockRate));
//        }

//        public byte[] Read(byte addressToReadFrom, int responseLength = 1)
//        {
//            var buffer = new byte[responseLength];
//            I2CTransaction[] transaction;
//            transaction = new I2CDevice.I2CTransaction[]
//            {
//                I2CDevice.CreateWriteTransaction(new byte[] { addressToReadFrom }),
//                I2CDevice.CreateReadTransaction(buffer)
//            };
//            int result = Execute(transaction, Timeout);
//            return buffer;
//        }

//        public void Write(byte addressToWriteTo, byte valueToWrite)
//        {
//            I2CTransaction[] transaction;
//            transaction = new I2CDevice.I2CTransaction[]
//            {
//                I2CDevice.CreateWriteTransaction(new byte[] { addressToWriteTo, valueToWrite })
//            };
//            int result = Execute(transaction, Timeout);
//        }

//        //public abstract bool Connected();

//        //public abstract byte[] DeviceIdentifier();
//    }
//}
//#endregion