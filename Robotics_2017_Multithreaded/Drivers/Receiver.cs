using System;
using System.Net.Sockets;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;
using RockSatC_2016.Utility;

namespace test_bot_netduino
{
    public class Receiver
    {
        private static I2CDevice.Configuration _slaveConfig;
        private const int TransactionTimeout = 1000;
        public byte address = 0x08;
        public static int clockSpeed = 100;

        public Receiver()
        {
            _slaveConfig = new I2CDevice.Configuration(address, clockSpeed);

        }

        public void Location()
        {
            var Loc = new byte[2];
            I2CBus.GetInstance().ReadRegister(_slaveConfig, address, Loc, TransactionTimeout);
            
            var MSB = Tools.Bcd2Bin(new[] { Loc[0] });
            var LSB = Tools.Bcd2Bin(new[] { Loc[1] });
            Debug.Print("Raw" + Loc[0] + " : " + Loc[1]);
            Debug.Print("I heard: " + MSB + " : " + LSB);
        }

        /*
        public void Location()
        {
            var Loc = new byte[2];
            I2CBus.GetInstance().ReadRegister(_slaveConfig, address, Loc, TransactionTimeout);
            //I2CBus.GetInstance().ReadRegister(_slaveConfig, 0x0D, Loc, TransactionTimeout);
            //I2CBus.GetInstance().ReadRegister(_slaveConfig, 0x08, Loc, TransactionTimeout);
            //I2CBus.GetInstance().Write(_slaveConfig, new byte[1], TransactionTimeout);
            //I2CBus.GetInstance().Read(_slaveConfig, Loc, TransactionTimeout);
            var MSB = Tools.Bcd2Bin(new[] { Loc[0] });
            var LSB = Tools.Bcd2Bin(new[] { Loc[1] });
            //Debug.Print("I heard: " + MSB + " : " + LSB);
            Debug.Print("I heard: " + Loc[0] + " : " + Loc[1]);
        }*/
       
        /*
            private static int location, MSB, LSB;

            public static void beacon()
            {
                location = 0;
                MSB = 0;
                LSB = 0;
            }

            public static void initialize()
        {

                //Wire.begin();
                I2CDevice.Configuration i2cConfiguration = new I2CDevice.Configuration(0x08, 400);
                I2CDevice i2CDevice = new I2CDevice(i2cConfiguration);

                I2CDevice.I2CTransaction[] xActions = new I2CDevice.I2CTransaction[2];

                //byte[] RegisterNum = new byte[1] { 2 };
                //xActions[0] = I2CDevice.CreateWriteTransaction(RegisterNum);
                // create read buffer to read the register
                //byte[] RegisterValue = new byte[1];
                //xActions[1] = I2CDevice.CreateReadTransaction(RegisterValue);
            }

         /*
        public static bool beaconPresent()
        {
            Wire.requestFrom(8, 2);//Refactor this
            MSB = Wire.read();
            LSB = Wire.read();
            int holder = (MSB << 8) | LSB;
            if (holder == -1)
            {
                return false;
            }
            return true;
        }


        public static void findBeacon()
        {
            int location;
            Wire.requestFrom(8, 2);
            MSB = Wire.read();
            LSB = Wire.read();
            int16_t holder = (MSB << 8) | LSB;//Points from beacon
                                              //Turn around to creat a heading to follow
            holder = holder - 180;
            if (holder >= 360) holder = holder - 360;
            else if (holder < 0) holder = holder + 360;
            location = holder; //Points to beacon
        }

        public static int returnLocation(int location)
        {
            return location;
        }*/
        }
}

