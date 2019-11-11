using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public enum Commands
    {
        GetVersion,
        Reset,
        RunBootloader,
        InitSerialPort,
        InitCan,
        SendOverSerialPort,
        SendOverCan,
        Shutdown,

        UnknownCommand,

    }
}
