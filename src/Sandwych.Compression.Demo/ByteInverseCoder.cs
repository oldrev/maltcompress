using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Sandwych.Compression.Demo
{
    public class ByteInverseCoder : BaseCoder, ICoder
    {
        private readonly int _blockSize;

        public ByteInverseCoder(string name, int blockSize) : base(name)
        {
            _blockSize = blockSize;
        }

        public void Code(Stream inStream, Stream outStream, ICodingProgress progress = null)
        {
            Console.WriteLine("{0} - start", this.Name);
            var buf = new byte[_blockSize];
            long processedSize = 0;
            for (;;)
            {
                int nRead = inStream.Read(buf, 0, buf.Length);
                if (nRead == 0)
                {
                    break;
                }
                outStream.Write(buf, 0, nRead);
                processedSize += nRead;

                if (progress != null)
                {
                    progress.Report(new CodingProgressInfo(processedSize, processedSize));
                }
            }
            Console.WriteLine("{0} - done", this.Name);
        }
    }
}
