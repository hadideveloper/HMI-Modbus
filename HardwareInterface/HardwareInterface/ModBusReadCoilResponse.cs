﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class ModBusReadCoilResponse : ModBusResponse
    {
        public byte ByteCount { set; get; }
        public byte[] Data { set; get; }
    }
}