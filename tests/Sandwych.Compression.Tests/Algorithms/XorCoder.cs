using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression.Tests.Algorithms
{
    public sealed class XorCoder : AbstractCoder
    {
        private readonly byte _key;
        private readonly Lazy<byte[]> _buffer;

        public byte Key => _key;

        public XorCoder(byte key, int bufferSize = 65536)
        {
            _buffer = new Lazy<byte[]>(() => new byte[bufferSize], true);
            _key = key;
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
                this.XorBlock(new Span<byte>(_buffer.Value, 0, nRead));
                outStream.Write(_buffer.Value, 0, nRead);
                processedSize += nRead;
                if (progress != null)
                {
                    progress.Report(new CodingProgressInfo(processedSize, processedSize));
                }
            }
        }

        private void XorBlock(Span<byte> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ _key);
            }
        }
    }
}
