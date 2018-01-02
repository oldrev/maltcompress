using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression
{
    public sealed class CopyCoder : AbstractCoder
    {
        private readonly Lazy<byte[]> _buffer;

        public CopyCoder(int bufferSize = 65536)
        {
            _buffer = new Lazy<byte[]>(() => new byte[bufferSize], true);
        }

        public override void Code(Stream inStream, Stream outStream, long inSize = -1, long outSize = -1, ICodingProgress progress = null)
        {
            long processedSize = 0;
            for (; ; )
            {
                var nRead = inStream.Read(_buffer.Value, 0, _buffer.Value.Length);
                if (nRead == 0)
                {
                    break;
                }
                outStream.Write(_buffer.Value, 0, nRead);
                processedSize += nRead;
                if (progress != null)
                {
                    progress.Report(new CodingProgressInfo(processedSize, processedSize));
                }
            }
        }
    }
}
