using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class HardwareInterface
    {
        private ComManager _cm;
        public int Port { set; get; }
        public int Baudrate { set; get; }
        public Commands[] SupportedCommands { set; get; }

        public event ReceiveNewPacketEvent OnReceiceNewPacket;

        public HardwareInterface(int port, int baudrate, Commands[] supportedCommands)
        {
            _cm = ComManager.Instance;
            Port = port;
            SupportedCommands = supportedCommands;
            Baudrate = baudrate;
        }


        public bool IsConnected
        {
            get
            {
                return _cm.IsConnected;
            }
        }

        public Result OpenHostSerialPort(string portName)
        {
            Result result = _cm.OpenSerialPort(portName);
            if (result.Success == false)
                return result;

            _cm.OnReceiveNewPacket += ComMan_OnReceivedPack;

            return _cm.Start();
        }

        public ComManager ComManager
        {
            get
            {
                return _cm;
            }
        }

        public Result InitSerialPort(int port, int baudrate, SerialPortMode mode)
        {
            return _cm.SendPacket(new Packet() { 
                Command = Commands.InitSerialPort,
                CommandParam = (byte)port,
                IsWaitForResponse = true,
                Data = new byte[] {
                    (byte)(baudrate >> 24),
                    (byte)(baudrate >> 16),
                    (byte)(baudrate >> 8), 
                    (byte)(baudrate),
                    (byte)mode,
                }
            });
        }

        public Result SendOverSerialPort(int port, byte[] message)
        {
            return _cm.SendPacket(new Packet() { 
                Command = Commands.SendOverSerialPort,
                CommandParam = (byte)port,
                Data = message,
                IsWaitForResponse = true,
            });
        }

        public Result SendOverSerialPort(int port, byte seqNum, byte[] message)
        {
            return _cm.SendPacket(new Packet()
            {
                Command = Commands.SendOverSerialPort,
                CommandParam = (byte)port,
                Data = message,
                IsWaitForResponse = true,
                SeqNum = seqNum, 
            });
        }

        private bool ComMan_OnReceivedPack(object sender, Packet p)
        {
            //! Check if it's supported Command for this interface
            if (SupportedCommands.Any(i => i == p.Command) == false)
                return false;

            //! Check if it's interface port
            if (p.CommandParam != Port)
                return false;

            OnReceiceNewPacket?.Invoke(this, p);

            return true;
        }

    }
}
