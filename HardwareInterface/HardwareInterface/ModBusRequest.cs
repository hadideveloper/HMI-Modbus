using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public abstract class ModBusRequest
    {
        public byte SlaveAddress { set; get; }
        public ushort StartAddress { set; get; }
    }
    public class ModBusReadRequest : ModBusRequest
    {
        public ushort NumberOfPoints { set; get; }
    }

    public class ModBusSingleWriteRequest : ModBusRequest
    {
        public ushort NewValue { set; get; }
    }

    public class ModBusMultiWriteRequest : ModBusRequest
    {
        public ushort[] NewValues { set; get; }
    }
}
