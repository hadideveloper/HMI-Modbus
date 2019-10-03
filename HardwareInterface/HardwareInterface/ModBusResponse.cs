using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HardwareInterface.ModBus;

namespace HardwareInterface
{
    public abstract class ModBusResponse
    {
        public ModBusMode Mode { set; get; }
        public byte SlaveAddress { set; get; }
        public ModbusFunctions Function { set; get; }
        
    }
}
