using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class ModBus
    {
        public enum ModBusMode
        {
            RTU,
            ASCII,
        }

        public ModBusMode Mode { set; get; } 
        public int Port { set; get; }
        public HardwareInterface HardwareInterface { set; get; }

        public ModBus()
        {
            HardwareInterface = HardwareInterface.Instance;
        }
        public ModBus(ModBusMode mode, int port)
        {
            Mode = mode;
            Port = port;

            HardwareInterface = HardwareInterface.Instance;
        }

        public Result Connect()
        {

            return HardwareInterface.OpenSerialPort(SettingManager.Instance.Settings.ComPort);
        }

        public Result ReadCoils(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {

            //byte
            return Result.Okay;
        }
    }
}
