using Modbus.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public class StreamResource : IStreamResource
    {
        public int InfiniteTimeout { set; get; }

        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }

        private byte[] buf;

        public void DiscardInBuffer()
        {
            
        }

        public void Dispose()
        {
            
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            buf = new byte[count];

            Array.Copy(buf, offset, buf, 0, count);
        }

        public byte[] Buffer
        {
            get
            {
                return buf;
            }
        }
    }
}
