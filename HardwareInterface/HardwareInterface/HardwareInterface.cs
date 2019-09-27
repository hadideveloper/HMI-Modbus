using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
using Modbus.Message;
using Modbus.IO;

namespace HardwareInterface
{
    public class HardwareInterface
    {
        private static HardwareInterface _instance;
        private ComManager _cm;

        private HardwareInterface()
        {
            _cm = ComManager.Instance;
        }

        public static HardwareInterface Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HardwareInterface();

                return _instance;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _cm.IsConnected;
            }
        }

        public Result OpenSerialPort(string portName)
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

        public void ModBusSend(int id)
        {
            
        } 

        private void ComMan_OnReceivedPack(object sender, Packet p)
        {
            
        }

    }
}
