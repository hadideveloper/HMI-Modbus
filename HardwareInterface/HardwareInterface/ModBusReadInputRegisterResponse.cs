using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class ModBusReadInputRegisterResponse : ModBusResponse
    {
        public byte ByteCount { set; get; }
        public ushort[] Data { set; get; }
    }
}
