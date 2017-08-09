// https://gist.github.com/Lordron/5039958

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sandwych.Compression.Algorithms;

namespace Sandwych.Compression.Algorithms.Bwt
{
    public class BwtEncoder : ICoder
    {
        public const int DefaultBlockSize = 1024 * 1024 * 2;
        private int[] _bucket;

        public void Code(Stream inStream, Stream outStream, ICodingProgress progress = null)
        {
            var inBuf = new byte[DefaultBlockSize];
            var outBuf = new byte[inBuf.Length];
            _bucket = new int[inBuf.Length];
            long processedSize = 0;
            for (;;)
            {
                var nRead = inStream.Read(inBuf, 0, inBuf.Length);
                if (nRead == 0)
                {
                    break;
                }
                processedSize += nRead;
                var primaryIndex = this.EncodeBlock(inBuf, outBuf, nRead);
                outStream.Write(outBuf, 0, nRead);
                if (progress != null)
                {
                    progress.Report(new CodingProgressInfo(processedSize, processedSize));
                }
            }
        }

        private int EncodeBlock(byte[] buf_in, byte[] buf_out, int size)
        {
            return SuffixArray.Bwt(buf_in, buf_out, _bucket, buf_in.Length);
        }
    }
}
