using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class ModBusRequest
    {
        public byte SlaveAddress { set; get; }
        public ushort StartAddress { set; get; }
        public ushort NumberOfPoints { set; get; }
    }
}
