using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class GeneralCommands
    {
        public HardwareInterface HardwareInterface { set; get; }
        public event ReceiceNewResponse OnReceiceNewResponse;

        public GeneralCommands()
        {
            HardwareInterface = new HardwareInterface(0, 0, new Commands[] { Commands.GetVersion, Commands.Reset, Commands.Shutdown, Commands.RunBootloader });

            HardwareInterface.OnReceiceNewPacket += HardwareInterface_OnReceiceNewPacket;
        }
        public Result Connect(string hostSerialPort)
        {
            if (string.IsNullOrEmpty(hostSerialPort))
                hostSerialPort = SettingManager.Instance.Settings.ComPort;

            return HardwareInterface.OpenHostSerialPort(hostSerialPort); 
        }
        private bool HardwareInterface_OnReceiceNewPacket(object sender, Packet packet)
        {
            if(packet.Command == Commands.GetVersion)
            {
                byte[] version = new byte[3];
                version[0] = packet.Data[0];
                version[1] = packet.Data[1];
                version[2] = packet.Data[2];

                OnReceiceNewResponse?.Invoke(this, Commands.GetVersion, version);
            }
            else if(packet.Command == Commands.Shutdown)
            {
                OnReceiceNewResponse?.Invoke(this, Commands.Shutdown, null);
            }

            return true;
        }
        public Result GetVersion()
        {
            HardwareInterface.ComManager.SendPacket(new Packet()
            {
                Command = Commands.GetVersion,
                CommandParam = 0,
                Data = null,
                IsWaitForResponse = true,
            });

            return Result.Okay;
        }
        public Result Shutdown()
        {
            HardwareInterface.ComManager.SendPacket(new Packet()
            {
                Command = Commands.Shutdown,
                CommandParam = 0,
                Data = null,
                IsWaitForResponse = true,
            });

            return Result.Okay;
        }
    }
}
