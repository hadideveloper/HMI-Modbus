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
        public ushort StartAddress { set; get; }

    }

    public class ModBusReadCoilResponse : ModBusResponse
    {
        public byte ByteCount { set; get; }
        public byte[] Data { set; get; }
    }

    public class ModBusReadHoldingRegisterResponse : ModBusResponse
    {
        public byte ByteCount { set; get; }
        public ushort[] Data { set; get; }
    }

    public class ModBusReadInputRegisterResponse : ModBusResponse
    {
        public byte ByteCount { set; get; }
        public ushort[] Data { set; get; }
    }

    public class ModBusReadInputResponse : ModBusResponse
    {
        public byte ByteCount { set; get; }
        public byte[] Data { set; get; }
    }

    public class ModBusWriteSingleResponse: ModBusResponse
    {
        public ushort Address { set; get; }
        public ushort Value { set; get; }
    }
}
