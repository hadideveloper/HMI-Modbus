using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class Packet
    {
        public Commands Command { set; get; }
        public byte CommandParam { set; get; }
        public byte[] Data { set; get; }
        public byte SeqNum { set; get; } = 0;
        public bool IsWaitForResponse { set; get; } = true;
    }
}
