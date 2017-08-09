using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Sandwych.Compression.Demo
{
    public class ByteBitNotCoder : BaseCoder, ICoder
    {
        private readonly int _blockSize;

        public ByteBitNotCoder(string name, int blockSize) : base(name)
        {
            _blockSize = blockSize;
        }

        public void Code(Stream inStream, Stream outStream, ICodingProgress progress = null)
        {
            Console.WriteLine("{0} - start", this.Name);
            var buf = new byte[_blockSize];
            int processed = 0;
            for (;;)
            {
                int nRead = inStream.Read(buf, 0, buf.Length);
                if (nRead == 0)
                {
                    break;
                }
                for (var i = 0; i < nRead; i++)
                {
                    buf[i] = (byte)(~buf[i]);
                }
                Console.WriteLine("{0} - {1} - read", this.Name, nRead);
                outStream.Write(buf, 0, nRead);
                Console.WriteLine("{0} - {1} - write", this.Name, nRead);
                processed += nRead;
            }
            Console.WriteLine("{0} - done", this.Name);
        }
    }
}
