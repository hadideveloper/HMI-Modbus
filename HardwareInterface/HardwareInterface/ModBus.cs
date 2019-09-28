using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Utility;

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
        public int Baudrate { set; get; }
        public HardwareInterface HardwareInterface { set; get; }
        public event ReceiveNewModBudResponse OnReceiveNewResponse;

        public ModBus(ModBusMode mode, int port, int baudrate)
        {
            Mode = mode;
            Port = port;
            Baudrate = baudrate;

            HardwareInterface = new HardwareInterface(port, baudrate, new Commands[] { Commands.SendOverSerialPort, Commands.InitSerialPort });

            HardwareInterface.OnReceiceNewPacket += HardwareInterface_OnReceiceNewPacket;
        }

        public Result Connect(string hostSerialPort)
        {
            if (string.IsNullOrEmpty(hostSerialPort))
                hostSerialPort = SettingManager.Instance.Settings.ComPort;

            Result openingPcSerialPortResult = HardwareInterface.OpenHostSerialPort(hostSerialPort);
            if (openingPcSerialPortResult.Success == false)
            {
                return openingPcSerialPortResult;
            }

            return HardwareInterface.InitSerialPort(Port, Baudrate);
        }

        private bool HardwareInterface_OnReceiceNewPacket(object sender, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length < 8)
                return false;



            OnReceiveNewResponse?.Invoke(this, new ModBusResponse());

            return true;
        }

        public Result ReadCoils(ModBusRequest req)
        {

            if (Mode == ModBusMode.RTU)
            {
                var message = new byte[8];

                message[0] = (byte)req.SlaveAddress;
                message[1] = (byte)ModbusFunctions.ReadCoils;
                message[2] = (byte)(req.StartAddress >> 8);
                message[3] = (byte)req.StartAddress;
                message[4] = (byte)(req.NumberOfPoints >> 8);
                message[5] = (byte)req.NumberOfPoints;

                var crc16 = ModbusUtility.ComputeCRC16(message, 0, message.Length-2);
                message[message.Length - 2] = crc16[0];
                message[message.Length - 1] = crc16[1];

                return HardwareInterface.SendOverSerialPort(Port, message);
            }
            
            return Result.NotImplemented;
        }

        
    }
}
